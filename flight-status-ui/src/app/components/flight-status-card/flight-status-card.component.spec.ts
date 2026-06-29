import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { FlightStatusCardComponent } from './flight-status-card.component';
import { FlightStatusResult } from '../../models/flight-status.model';

function createFlightResult(overrides: Partial<FlightStatusResult> = {}): FlightStatusResult {
  return {
    flightNumber: 'BA123',
    date: '2024-03-15',
    status: 'OnTime',
    scheduledDeparture: '2024-03-15T08:00:00Z',
    scheduledArrival: '2024-03-15T11:30:00Z',
    actualDeparture: null,
    actualArrival: null,
    terminal: null,
    gate: null,
    delayReason: null,
    lastUpdatedUtc: '2024-03-15T11:30:00Z',
    provider: 'AeroTrack',
    message: null,
    ...overrides,
  };
}

@Component({
  imports: [FlightStatusCardComponent],
  template: `<app-flight-status-card [flight]="flight()" />`,
})
class TestHostComponent {
  flight = signal<FlightStatusResult>(createFlightResult());
}

describe('FlightStatusCardComponent', () => {
  let hostComponent: TestHostComponent;
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    hostComponent = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should render OnTime with status-ontime class', () => {
    hostComponent.flight.set(createFlightResult({ status: 'OnTime' }));
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.status-badge');
    expect(badge.classList.contains('status-ontime')).toBe(true);
    expect(badge.textContent?.trim()).toBe('OnTime');
  });

  it('should render Delayed with status-delayed class', () => {
    hostComponent.flight.set(createFlightResult({ status: 'Delayed' }));
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.status-badge');
    expect(badge.classList.contains('status-delayed')).toBe(true);
    expect(badge.textContent?.trim()).toBe('Delayed');
  });

  it('should render Cancelled with status-cancelled class', () => {
    hostComponent.flight.set(createFlightResult({ status: 'Cancelled' }));
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.status-badge');
    expect(badge.classList.contains('status-cancelled')).toBe(true);
    expect(badge.textContent?.trim()).toBe('Cancelled');
  });

  it('should render Diverted with status-diverted class', () => {
    hostComponent.flight.set(createFlightResult({ status: 'Diverted' }));
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.status-badge');
    expect(badge.classList.contains('status-diverted')).toBe(true);
    expect(badge.textContent?.trim()).toBe('Diverted');
  });

  it('should render Unknown with status-unknown class', () => {
    hostComponent.flight.set(createFlightResult({ status: 'Unknown' }));
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.status-badge');
    expect(badge.classList.contains('status-unknown')).toBe(true);
    expect(badge.textContent?.trim()).toBe('Unknown');
  });

  it('should show terminal, gate, and delayReason when present', () => {
    hostComponent.flight.set(createFlightResult({
      terminal: '5',
      gate: 'B42',
      delayReason: 'Weather conditions',
    }));
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('5');
    expect(compiled.textContent).toContain('B42');
    expect(compiled.textContent).toContain('Weather conditions');
  });

  it('should hide terminal, gate, and delayReason when null', () => {
    hostComponent.flight.set(createFlightResult({
      terminal: null,
      gate: null,
      delayReason: null,
    }));
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labels = Array.from(compiled.querySelectorAll('.label'));
    const labelTexts = labels.map(l => l.textContent?.trim().toUpperCase());

    expect(labelTexts).not.toContain('TERMINAL');
    expect(labelTexts).not.toContain('GATE');
    expect(labelTexts).not.toContain('DELAY REASON');
  });
});

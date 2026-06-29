import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';
import { FlightStatusResult } from './models/flight-status.model';

describe('AppComponent', () => {
  let component: App;
  let fixture: ComponentFixture<App>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;
  });

  it('should create the app', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render the title', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Flight Status Tracker');
  });

  it('should show loading indicator when loading is true', () => {
    component.loading = true;
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const loadingEl = compiled.querySelector('.loading-indicator');

    expect(loadingEl).toBeTruthy();
    expect(loadingEl!.textContent).toContain('Loading');
  });

  it('should show result card when result is available', () => {
    const mockResult: FlightStatusResult = {
      flightNumber: 'BA123',
      date: '2024-03-15',
      status: 'OnTime',
      scheduledDeparture: '2024-03-15T08:00:00Z',
      scheduledArrival: '2024-03-15T11:30:00Z',
      actualDeparture: '2024-03-15T08:05:00Z',
      actualArrival: '2024-03-15T11:28:00Z',
      terminal: '5',
      gate: 'B42',
      delayReason: null,
      lastUpdatedUtc: '2024-03-15T11:30:00Z',
      provider: 'AeroTrack',
      message: null,
    };

    component.result = mockResult;
    component.loading = false;
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-flight-status-card')).toBeTruthy();
  });

  it('should show error display when error is set', () => {
    component.error = 'Flight not found';
    component.loading = false;
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-error-display')).toBeTruthy();
    expect(compiled.textContent).toContain('Flight not found');
  });

  it('should not show result or error while loading', () => {
    component.loading = true;
    component.result = {
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
    };
    component.error = 'Some error';
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-flight-status-card')).toBeFalsy();
    expect(compiled.querySelector('app-error-display')).toBeFalsy();
  });
});

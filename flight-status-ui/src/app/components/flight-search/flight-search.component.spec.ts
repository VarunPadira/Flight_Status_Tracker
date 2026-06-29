import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FlightSearchComponent } from './flight-search.component';

describe('FlightSearchComponent', () => {
  let component: FlightSearchComponent;
  let fixture: ComponentFixture<FlightSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FlightSearchComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FlightSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should render flightNumber and date input fields', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const flightInput = compiled.querySelector('input#flightNumber');
    const dateInput = compiled.querySelector('input#date');

    expect(flightInput).toBeTruthy();
    expect(dateInput).toBeTruthy();
  });

  it('should render a search button', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector('button[type="submit"]');

    expect(button).toBeTruthy();
    expect(button!.textContent).toContain('Search');
  });

  it('should show validation message when flightNumber is empty and touched', () => {
    component.searchForm.controls.flightNumber.markAsTouched();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const errorMessage = compiled.querySelector('.error-message');

    expect(errorMessage).toBeTruthy();
    expect(errorMessage!.textContent).toContain('Flight number is required');
  });

  it('should show validation message when date is empty and touched', () => {
    component.searchForm.controls.date.markAsTouched();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const errorMessages = compiled.querySelectorAll('.error-message');
    const dateError = Array.from(errorMessages).find(el =>
      el.textContent?.includes('Date is required')
    );

    expect(dateError).toBeTruthy();
  });

  it('should emit search event with correct values on valid form submission', () => {
    let emittedValue: { flightNumber: string; date: string } | undefined;
    component.search.subscribe((value) => {
      emittedValue = value;
    });

    component.searchForm.controls.flightNumber.setValue('BA123');
    component.searchForm.controls.date.setValue('2024-03-15');
    component.onSubmit();

    expect(emittedValue).toEqual({ flightNumber: 'BA123', date: '2024-03-15' });
  });

  it('should NOT emit search event on invalid form submission', () => {
    let emitted = false;
    component.search.subscribe(() => {
      emitted = true;
    });

    component.onSubmit();

    expect(emitted).toBe(false);
  });
});

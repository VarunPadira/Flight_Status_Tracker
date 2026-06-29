import { Component, output } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-flight-search',
  imports: [ReactiveFormsModule],
  template: `
    <div class="search-card">
      <form [formGroup]="searchForm" (ngSubmit)="onSubmit()">
        <div class="form-row">
          <div class="form-field">
            <label for="flightNumber">
              <span class="field-icon">🛫</span>
              Flight Number
            </label>
            <input
              id="flightNumber"
              type="text"
              formControlName="flightNumber"
              placeholder="e.g. BA123"
            />
            @if (searchForm.controls.flightNumber.invalid && searchForm.controls.flightNumber.touched) {
              <span class="error-message">Flight number is required.</span>
            }
          </div>

          <div class="form-field">
            <label for="date">
              <span class="field-icon">📅</span>
              Date
            </label>
            <input
              id="date"
              type="date"
              formControlName="date"
            />
            @if (searchForm.controls.date.invalid && searchForm.controls.date.touched) {
              <span class="error-message">Date is required.</span>
            }
          </div>
        </div>

        <button type="submit">
          <span class="btn-icon">🔍</span>
          Search Flight
        </button>
      </form>
    </div>
  `,
  styles: `
    .search-card {
      background: white;
      border-radius: var(--radius-xl, 1.5rem);
      padding: 2rem;
      box-shadow: var(--shadow-xl, 0 20px 25px -5px rgba(0, 0, 0, 0.1));
      border: 1px solid rgba(255, 255, 255, 0.8);
    }

    form {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .form-row {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    @media (min-width: 600px) {
      .form-row {
        flex-direction: row;
        gap: 1.5rem;
      }

      .form-field {
        flex: 1;
      }
    }

    .form-field {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    label {
      font-size: 0.8rem;
      font-weight: 600;
      color: #475569;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      display: flex;
      align-items: center;
      gap: 0.375rem;
    }

    .field-icon {
      font-size: 0.875rem;
    }

    input {
      padding: 0.875rem 1rem;
      border: 2px solid #e2e8f0;
      border-radius: 0.75rem;
      font-size: 1rem;
      width: 100%;
      transition: all 0.2s ease;
      background: #f8fafc;
      font-weight: 500;
    }

    input:hover {
      border-color: #cbd5e1;
      background: white;
    }

    input:focus {
      outline: none;
      border-color: #0ea5e9;
      background: white;
      box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.1);
    }

    input::placeholder {
      color: #94a3b8;
      font-weight: 400;
    }

    .error-message {
      color: #ef4444;
      font-size: 0.75rem;
      font-weight: 500;
    }

    button {
      padding: 1rem 2rem;
      background: var(--gradient-accent, linear-gradient(135deg, #0ea5e9, #6366f1));
      color: white;
      border: none;
      border-radius: 0.75rem;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      box-shadow: 0 4px 12px rgba(14, 165, 233, 0.3);
    }

    button:hover {
      transform: translateY(-1px);
      box-shadow: 0 8px 20px rgba(14, 165, 233, 0.4);
    }

    button:active {
      transform: translateY(0);
      box-shadow: 0 2px 8px rgba(14, 165, 233, 0.3);
    }

    .btn-icon {
      font-size: 1rem;
    }
  `
})
export class FlightSearchComponent {
  search = output<{ flightNumber: string; date: string }>();

  searchForm = new FormGroup({
    flightNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    date: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  onSubmit(): void {
    if (this.searchForm.valid) {
      this.search.emit({
        flightNumber: this.searchForm.controls.flightNumber.value,
        date: this.searchForm.controls.date.value,
      });
    } else {
      this.searchForm.markAllAsTouched();
    }
  }
}

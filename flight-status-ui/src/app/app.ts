import { Component, inject, signal } from '@angular/core';
import { FlightSearchComponent } from './components/flight-search/flight-search.component';
import { FlightStatusCardComponent } from './components/flight-status-card/flight-status-card.component';
import { ErrorDisplayComponent } from './components/error-display/error-display.component';
import { FlightStatusService } from './services/flight-status.service';
import { FlightStatusResult } from './models/flight-status.model';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-root',
  imports: [FlightSearchComponent, FlightStatusCardComponent, ErrorDisplayComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly flightStatusService = inject(FlightStatusService);

  loading = signal(false);
  result = signal<FlightStatusResult | null>(null);
  error = signal<string | null>(null);

  onSearch(event: { flightNumber: string; date: string }): void {
    this.loading.set(true);
    this.result.set(null);
    this.error.set(null);

    this.flightStatusService.getFlightStatus(event.flightNumber, event.date).subscribe({
      next: (data) => {
        this.result.set(data);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        if (err.error && typeof err.error === 'object' && err.error.message) {
          this.error.set(err.error.message);
        } else if (err.message) {
          this.error.set(err.message);
        } else {
          this.error.set('An unexpected error occurred. Please try again.');
        }
        this.loading.set(false);
      }
    });
  }
}

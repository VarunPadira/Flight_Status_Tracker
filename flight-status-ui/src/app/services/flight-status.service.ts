import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FlightStatusResult } from '../models/flight-status.model';

@Injectable({
  providedIn: 'root'
})
export class FlightStatusService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5007';

  getFlightStatus(flightNumber: string, date: string): Observable<FlightStatusResult> {
    const params = new HttpParams()
      .set('flightNumber', flightNumber)
      .set('date', date);
    return this.http.get<FlightStatusResult>(`${this.baseUrl}/flights/status`, { params });
  }
}

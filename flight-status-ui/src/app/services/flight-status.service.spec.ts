import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { FlightStatusService } from './flight-status.service';
import { FlightStatusResult } from '../models/flight-status.model';

describe('FlightStatusService', () => {
  let service: FlightStatusService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(FlightStatusService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should call the correct URL with query params', () => {
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

    service.getFlightStatus('BA123', '2024-03-15').subscribe((result) => {
      expect(result).toEqual(mockResult);
    });

    const req = httpTesting.expectOne(
      (r) =>
        r.url === 'http://localhost:5000/flights/status' &&
        r.params.get('flightNumber') === 'BA123' &&
        r.params.get('date') === '2024-03-15'
    );

    expect(req.request.method).toBe('GET');
    req.flush(mockResult);
  });

  it('should pass different flight numbers and dates', () => {
    service.getFlightStatus('LH456', '2024-06-20').subscribe();

    const req = httpTesting.expectOne(
      (r) =>
        r.url === 'http://localhost:5000/flights/status' &&
        r.params.get('flightNumber') === 'LH456' &&
        r.params.get('date') === '2024-06-20'
    );

    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});

export interface FlightStatusResult {
  flightNumber: string;
  date: string;
  status: 'OnTime' | 'Delayed' | 'Cancelled' | 'Diverted' | 'Unknown';
  scheduledDeparture: string;
  scheduledArrival: string;
  actualDeparture: string | null;
  actualArrival: string | null;
  terminal: string | null;
  gate: string | null;
  delayReason: string | null;
  lastUpdatedUtc: string;
  provider: string;
  message: string | null;
}

export interface ErrorResponse {
  message: string;
  field: string | null;
}

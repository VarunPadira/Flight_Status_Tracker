import { Component, input } from '@angular/core';
import { NgClass } from '@angular/common';
import { FlightStatusResult } from '../../models/flight-status.model';

@Component({
  selector: 'app-flight-status-card',
  imports: [NgClass],
  template: `
    <div class="card" [ngClass]="cardClass()">
      <div class="card-header">
        <div class="header-left">
          <span class="flight-number">{{ flight().flightNumber }}</span>
          <span class="flight-date">{{ flight().date }}</span>
        </div>
        <div class="status-badge" [ngClass]="statusClass()">
          <span class="status-dot"></span>
          {{ flight().status }}
        </div>
      </div>

      <div class="card-body">
        <div class="timeline">
          <div class="timeline-point departure">
            <div class="time-block">
              <span class="time-label">Departure</span>
              <span class="time-value">{{ formatTime(flight().scheduledDeparture) }}</span>
              @if (flight().actualDeparture) {
                <span class="time-actual">Actual: {{ formatTime(flight().actualDeparture!) }}</span>
              }
            </div>
          </div>
          <div class="timeline-line">
            <span class="plane-icon">✈</span>
          </div>
          <div class="timeline-point arrival">
            <div class="time-block">
              <span class="time-label">Arrival</span>
              <span class="time-value">{{ formatTime(flight().scheduledArrival) }}</span>
              @if (flight().actualArrival) {
                <span class="time-actual">Actual: {{ formatTime(flight().actualArrival!) }}</span>
              }
            </div>
          </div>
        </div>

        @if (flight().terminal || flight().gate || flight().delayReason) {
          <div class="details-grid">
            @if (flight().terminal) {
              <div class="detail-chip">
                <span class="chip-icon">🏢</span>
                <div class="chip-content">
                  <span class="chip-label">Terminal</span>
                  <span class="chip-value">{{ flight().terminal }}</span>
                </div>
              </div>
            }
            @if (flight().gate) {
              <div class="detail-chip">
                <span class="chip-icon">🚪</span>
                <div class="chip-content">
                  <span class="chip-label">Gate</span>
                  <span class="chip-value">{{ flight().gate }}</span>
                </div>
              </div>
            }
            @if (flight().delayReason) {
              <div class="detail-chip delay">
                <span class="chip-icon">⚠️</span>
                <div class="chip-content">
                  <span class="chip-label">Delay Reason</span>
                  <span class="chip-value">{{ flight().delayReason }}</span>
                </div>
              </div>
            }
          </div>
        }

        <div class="card-footer">
          <span class="provider-tag">{{ flight().provider }}</span>
          <span class="updated-time">Updated {{ formatTime(flight().lastUpdatedUtc) }}</span>
        </div>
      </div>
    </div>
  `,
  styles: `
    .card {
      border-radius: var(--radius-xl, 1.5rem);
      overflow: hidden;
      box-shadow: var(--shadow-lg, 0 10px 15px -3px rgba(0, 0, 0, 0.1));
      background: white;
      border: 1px solid #e2e8f0;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    .card:hover {
      transform: translateY(-2px);
      box-shadow: var(--shadow-xl, 0 20px 25px -5px rgba(0, 0, 0, 0.1));
    }

    .card-border-ontime { border-top: 4px solid var(--color-ontime, #10b981); }
    .card-border-delayed { border-top: 4px solid var(--color-delayed, #f59e0b); }
    .card-border-cancelled { border-top: 4px solid var(--color-cancelled, #ef4444); }
    .card-border-diverted { border-top: 4px solid var(--color-diverted, #8b5cf6); }
    .card-border-unknown { border-top: 4px solid var(--color-unknown, #6b7280); }

    .card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1.5rem 1.5rem 1rem;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .header-left {
      display: flex;
      align-items: baseline;
      gap: 0.75rem;
    }

    .flight-number {
      font-size: 1.5rem;
      font-weight: 800;
      color: #0f172a;
      letter-spacing: -0.025em;
    }

    .flight-date {
      font-size: 0.875rem;
      color: #64748b;
      font-weight: 500;
    }

    .status-badge {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      border-radius: 9999px;
      font-size: 0.8rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.025em;
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
      animation: pulse-dot 2s ease-in-out infinite;
    }

    @keyframes pulse-dot {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }

    .status-ontime {
      background: var(--color-ontime-bg, #ecfdf5);
      color: var(--color-ontime, #10b981);
      border: 1px solid var(--color-ontime-border, #a7f3d0);
    }

    .status-delayed {
      background: var(--color-delayed-bg, #fffbeb);
      color: var(--color-delayed, #f59e0b);
      border: 1px solid var(--color-delayed-border, #fde68a);
    }

    .status-cancelled {
      background: var(--color-cancelled-bg, #fef2f2);
      color: var(--color-cancelled, #ef4444);
      border: 1px solid var(--color-cancelled-border, #fecaca);
    }

    .status-diverted {
      background: var(--color-diverted-bg, #f5f3ff);
      color: var(--color-diverted, #8b5cf6);
      border: 1px solid var(--color-diverted-border, #ddd6fe);
    }

    .status-unknown {
      background: var(--color-unknown-bg, #f9fafb);
      color: var(--color-unknown, #6b7280);
      border: 1px solid var(--color-unknown-border, #e5e7eb);
    }

    .card-body {
      padding: 0 1.5rem 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    /* Timeline */
    .timeline {
      display: flex;
      align-items: center;
      gap: 0;
      padding: 1.25rem;
      background: #f8fafc;
      border-radius: 1rem;
    }

    .timeline-point {
      flex: 1;
    }

    .timeline-point.arrival {
      text-align: right;
    }

    .timeline-line {
      flex: 0 0 auto;
      width: 80px;
      height: 2px;
      background: linear-gradient(90deg, #cbd5e1, #0ea5e9, #cbd5e1);
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 1rem;
    }

    .plane-icon {
      position: absolute;
      font-size: 1rem;
      animation: fly 2s ease-in-out infinite;
    }

    @keyframes fly {
      0%, 100% { transform: translateX(-8px); }
      50% { transform: translateX(8px); }
    }

    .time-block {
      display: flex;
      flex-direction: column;
      gap: 0.125rem;
    }

    .time-label {
      font-size: 0.7rem;
      color: #94a3b8;
      text-transform: uppercase;
      font-weight: 600;
      letter-spacing: 0.05em;
    }

    .time-value {
      font-size: 0.9rem;
      font-weight: 700;
      color: #0f172a;
    }

    .time-actual {
      font-size: 0.75rem;
      color: #64748b;
      font-weight: 500;
    }

    /* Details Grid */
    .details-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .detail-chip {
      display: flex;
      align-items: center;
      gap: 0.625rem;
      padding: 0.75rem 1rem;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 0.75rem;
      flex: 1;
      min-width: 140px;
    }

    .detail-chip.delay {
      background: #fef2f2;
      border-color: #fecaca;
      flex-basis: 100%;
    }

    .chip-icon {
      font-size: 1.25rem;
      flex-shrink: 0;
    }

    .chip-content {
      display: flex;
      flex-direction: column;
      gap: 0.125rem;
    }

    .chip-label {
      font-size: 0.7rem;
      color: #94a3b8;
      text-transform: uppercase;
      font-weight: 600;
    }

    .chip-value {
      font-size: 0.875rem;
      font-weight: 600;
      color: #1e293b;
    }

    .detail-chip.delay .chip-value {
      color: #ef4444;
    }

    /* Footer */
    .card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-top: 1rem;
      border-top: 1px solid #f1f5f9;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .provider-tag {
      font-size: 0.75rem;
      font-weight: 600;
      color: #64748b;
      background: #f1f5f9;
      padding: 0.25rem 0.75rem;
      border-radius: 9999px;
    }

    .updated-time {
      font-size: 0.75rem;
      color: #94a3b8;
    }

    @media (max-width: 480px) {
      .timeline {
        flex-direction: column;
        gap: 0.75rem;
        align-items: stretch;
      }

      .timeline-point.arrival {
        text-align: left;
      }

      .timeline-line {
        width: 100%;
        height: 2px;
        margin: 0.5rem 0;
      }
    }
  `
})
export class FlightStatusCardComponent {
  flight = input.required<FlightStatusResult>();

  statusClass(): string {
    switch (this.flight().status) {
      case 'OnTime':
        return 'status-ontime';
      case 'Delayed':
        return 'status-delayed';
      case 'Cancelled':
        return 'status-cancelled';
      case 'Diverted':
        return 'status-diverted';
      case 'Unknown':
      default:
        return 'status-unknown';
    }
  }

  cardClass(): string {
    switch (this.flight().status) {
      case 'OnTime':
        return 'card-border-ontime';
      case 'Delayed':
        return 'card-border-delayed';
      case 'Cancelled':
        return 'card-border-cancelled';
      case 'Diverted':
        return 'card-border-diverted';
      case 'Unknown':
      default:
        return 'card-border-unknown';
    }
  }

  formatTime(isoString: string): string {
    try {
      const date = new Date(isoString);
      return date.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', hour12: false }) +
        ' · ' + date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' });
    } catch {
      return isoString;
    }
  }
}

import { Component, input } from '@angular/core';

@Component({
  selector: 'app-error-display',
  template: `
    <div class="error-container" role="alert">
      <div class="error-icon-wrapper">
        <span class="error-icon">!</span>
      </div>
      <div class="error-body">
        <span class="error-title">Something went wrong</span>
        <span class="error-message">{{ message() }}</span>
      </div>
    </div>
  `,
  styles: `
    .error-container {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
      padding: 1.25rem 1.5rem;
      border: 1px solid #fecaca;
      border-radius: var(--radius-lg, 1rem);
      background: linear-gradient(135deg, #fef2f2, #fff1f2);
      box-shadow: 0 4px 12px rgba(239, 68, 68, 0.1);
    }

    .error-icon-wrapper {
      flex-shrink: 0;
      width: 2.25rem;
      height: 2.25rem;
      border-radius: 50%;
      background: #ef4444;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .error-icon {
      color: white;
      font-weight: 800;
      font-size: 1rem;
    }

    .error-body {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .error-title {
      font-size: 0.875rem;
      font-weight: 700;
      color: #991b1b;
    }

    .error-message {
      font-size: 0.8rem;
      color: #b91c1c;
      line-height: 1.5;
    }
  `
})
export class ErrorDisplayComponent {
  message = input.required<string>();
}

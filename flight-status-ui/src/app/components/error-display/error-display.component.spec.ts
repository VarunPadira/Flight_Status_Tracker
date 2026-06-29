import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { ErrorDisplayComponent } from './error-display.component';

@Component({
  imports: [ErrorDisplayComponent],
  template: `<app-error-display [message]="message()" />`,
})
class TestHostComponent {
  message = signal('Something went wrong');
}

describe('ErrorDisplayComponent', () => {
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

  it('should render the error message', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const errorMessage = compiled.querySelector('.error-message');

    expect(errorMessage).toBeTruthy();
    expect(errorMessage!.textContent).toContain('Something went wrong');
  });

  it('should render with role alert for accessibility', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('[role="alert"]');

    expect(container).toBeTruthy();
  });

  it('should update when message changes', () => {
    hostComponent.message.set('Network error occurred');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Network error occurred');
  });
});

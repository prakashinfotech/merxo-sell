import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ToastService } from '../../../core/toast/toast.service';

/**
 * Renders the active toast stack. Mounted once in app.html.
 * Includes an animated progress bar that shrinks over the toast's durationMs.
 */
@Component({
  selector: 'app-toast-host',
  standalone: false,
  template: `
    <div class="toast-host" aria-live="polite" aria-atomic="true">
      @for (t of toast.toasts(); track t.id) {
        <div class="toast" [class]="'toast--' + t.variant" [class.toast--leaving]="t.leaving" role="status">
          <div class="toast__icon">
            <i class="fas"
               [class.fa-check-circle]="t.variant === 'success'"
               [class.fa-circle-exclamation]="t.variant === 'error'"
               [class.fa-circle-info]="t.variant === 'info'"
               [class.fa-triangle-exclamation]="t.variant === 'warn'"></i>
          </div>
          <div class="toast__body">
            <span class="toast__title">{{ t.title }}</span>
            @if (t.message) { <span class="toast__message">{{ t.message }}</span> }
          </div>
          <button class="toast__close" type="button" (click)="toast.dismiss(t.id)" aria-label="Dismiss">
            <i class="fas fa-times"></i>
          </button>
          @if (t.durationMs > 0) {
            <div class="toast__progress"
                 [style.animation-duration]="t.durationMs + 'ms'"></div>
          }
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastHost {
  constructor(public toast: ToastService) {}
}

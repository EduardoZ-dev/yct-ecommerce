import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-host',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-stack" aria-live="polite">
      @for (t of toast.toasts(); track t.id) {
        <div class="toast" [class]="t.kind" (click)="toast.dismiss(t.id)" role="status">
          <span class="dot"></span>
          <span class="msg">{{ t.message }}</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-stack {
      position: fixed;
      bottom: 16px;
      right: 16px;
      z-index: 1000;
      display: flex;
      flex-direction: column;
      gap: 8px;
      max-width: calc(100vw - 32px);
    }
    .toast {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 16px;
      background: var(--bg-surface);
      border: 1px solid var(--border-color);
      border-left: 4px solid var(--brand-green-dark);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-lg);
      color: var(--text-primary);
      font-size: 14px;
      min-width: 240px;
      max-width: 380px;
      cursor: pointer;
      animation: slideIn .2s ease;
    }
    .toast.success { border-left-color: var(--success); }
    .toast.error   { border-left-color: var(--danger); }
    .toast.warning { border-left-color: var(--warning); }
    .toast.info    { border-left-color: var(--brand-teal); }
    .dot {
      width: 8px; height: 8px; border-radius: 50%;
      background: var(--brand-green-dark); flex-shrink: 0;
    }
    .toast.success .dot { background: var(--success); }
    .toast.error .dot   { background: var(--danger); }
    .toast.warning .dot { background: var(--warning); }
    .toast.info .dot    { background: var(--brand-teal); }
    .msg { flex: 1; }
    @keyframes slideIn {
      from { opacity: 0; transform: translateX(100%); }
      to { opacity: 1; transform: translateX(0); }
    }
  `]
})
export class ToastHostComponent {
  toast = inject(ToastService);
}

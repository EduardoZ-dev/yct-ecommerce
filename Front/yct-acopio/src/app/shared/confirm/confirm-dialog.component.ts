import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from './confirm.service';
import { IconComponent } from '../../components/icon/icon.component';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    @if (svc.state(); as s) {
      <div class="confirm-backdrop" (click)="svc.cancel()">
        <div class="confirm-modal" (click)="$event.stopPropagation()">
          <div class="confirm-icon" [class.danger]="s.danger">
            <app-icon [name]="s.icon ?? (s.danger ? 'trash' : 'warning')" [size]="28"/>
          </div>
          <h3 class="confirm-title">{{ s.title }}</h3>
          <p class="confirm-msg">{{ s.message }}</p>
          <div class="confirm-actions">
            <button class="btn-secondary" (click)="svc.cancel()">
              {{ s.cancelText || 'Cancelar' }}
            </button>
            <button class="btn-primary" [class.btn-danger]="s.danger" (click)="svc.accept()">
              {{ s.confirmText || 'Confirmar' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .confirm-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(30, 50, 20, 0.45);
      backdrop-filter: blur(14px) saturate(140%);
      -webkit-backdrop-filter: blur(14px) saturate(140%);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 16px;
      animation: cFade .2s ease;
    }
    .confirm-modal {
      width: 100%;
      max-width: 380px;
      background: linear-gradient(160deg,
                  rgba(255, 255, 255, 0.96) 0%,
                  rgba(237, 247, 229, 0.92) 100%);
      border: 1px solid rgba(122, 182, 72, 0.3);
      border-radius: 24px;
      box-shadow:
        0 20px 60px rgba(30, 60, 20, 0.3),
        0 4px 16px rgba(30, 60, 20, 0.1),
        inset 0 1px 0 rgba(255, 255, 255, 0.9);
      padding: 28px 24px 20px;
      text-align: center;
      animation: cPop .25s cubic-bezier(0.34, 1.4, 0.64, 1);
    }
    .confirm-icon {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: rgba(122, 182, 72, 0.15);
      color: var(--brand-green-dark);
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 14px;
      &.danger {
        background: rgba(229, 57, 53, 0.12);
        color: var(--danger);
      }
    }
    .confirm-title {
      margin: 0 0 8px;
      font-size: 18px;
      color: var(--text-primary);
      font-weight: 700;
    }
    .confirm-msg {
      margin: 0 0 20px;
      color: var(--text-secondary);
      font-size: 14px;
      line-height: 1.5;
    }
    .confirm-actions {
      display: flex;
      gap: 10px;
      button {
        flex: 1;
        padding: 10px 16px;
        font-size: 14px;
      }
    }
    .btn-danger {
      background: var(--danger) !important;
      border-color: var(--danger) !important;
      color: #ffffff !important;
      font-weight: 700;
      box-shadow: 0 4px 14px rgba(229, 57, 53, 0.35);
      &:hover {
        background: #c62828 !important;
        border-color: #c62828 !important;
        color: #ffffff !important;
      }
    }
    @keyframes cFade { from { opacity: 0; } to { opacity: 1; } }
    @keyframes cPop {
      from { opacity: 0; transform: scale(0.9) translateY(10px); }
      to { opacity: 1; transform: scale(1) translateY(0); }
    }
  `]
})
export class ConfirmDialogComponent {
  svc = inject(ConfirmService);
}

import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  danger?: boolean;
  icon?: 'trash' | 'warning' | 'check-circle';
}

interface ConfirmState extends ConfirmOptions {
  resolve: (ok: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  state = signal<ConfirmState | null>(null);

  ask(opts: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.state.set({ ...opts, resolve });
    });
  }

  accept(): void {
    const s = this.state();
    if (!s) return;
    s.resolve(true);
    this.state.set(null);
  }

  cancel(): void {
    const s = this.state();
    if (!s) return;
    s.resolve(false);
    this.state.set(null);
  }
}

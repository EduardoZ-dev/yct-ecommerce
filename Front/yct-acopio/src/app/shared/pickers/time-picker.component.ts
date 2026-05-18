import {
  AfterViewChecked, Component, EventEmitter, Input, Output, signal,
  TemplateRef, ViewChild, ViewContainerRef, EmbeddedViewRef,
  effect, inject, OnDestroy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../components/icon/icon.component';

@Component({
  selector: 'app-time-picker',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <input type="text" readonly class="picker-input"
           [value]="displayLabel()"
           (click)="open($event)"
           [placeholder]="placeholder">

    <ng-template #modalTpl>
      <div class="picker-backdrop" (click)="close()">
        <div class="picker-modal" (click)="$event.stopPropagation()">
          <header class="picker-head">
            <div>
              <small>Selecciona hora</small>
              <h3>{{ title || label }}</h3>
            </div>
            <button class="close-btn" (click)="close()" aria-label="Cerrar">
              <app-icon name="close" [size]="20"/>
            </button>
          </header>

          <div class="picker-body">
            <div class="ampm-toggle">
              <button type="button" [class.active]="timeAmPm() === 'AM'" (click)="setAmPm('AM')">AM</button>
              <button type="button" [class.active]="timeAmPm() === 'PM'" (click)="setAmPm('PM')">PM</button>
            </div>

            <div class="wheel-cal">
              <div class="wheel-stripe"></div>
              <div class="wheel-col" data-wheel="h" (scroll)="onWheelScroll('h', $event)">
                <div class="wheel-pad"></div>
                @for (h of hourList; track h) {
                  <div class="wheel-item" [class.active]="h === timeHour()" (click)="pickWheel('h', h, $event)">
                    {{ h.toString().padStart(2, '0') }}
                  </div>
                }
                <div class="wheel-pad"></div>
              </div>
              <div class="wheel-sep">:</div>
              <div class="wheel-col" data-wheel="m" (scroll)="onWheelScroll('m', $event)">
                <div class="wheel-pad"></div>
                @for (m of minuteList; track m) {
                  <div class="wheel-item" [class.active]="m === timeMinute()" (click)="pickWheel('m', m, $event)">
                    {{ m.toString().padStart(2, '0') }}
                  </div>
                }
                <div class="wheel-pad"></div>
              </div>
            </div>
          </div>

          <footer class="picker-foot">
            <button class="btn-secondary" (click)="close()">Cancelar</button>
            <button class="btn-primary" (click)="confirm()">Confirmar hora</button>
          </footer>
        </div>
      </div>
    </ng-template>
  `
})
export class TimePickerComponent implements OnDestroy, AfterViewChecked {
  @Input() value: string = '';
  @Input() label = '';
  @Input() title = '';
  @Input() placeholder = '--:-- --';
  @Output() valueChange = new EventEmitter<string>();

  @ViewChild('modalTpl') modalTpl!: TemplateRef<any>;
  private vcr = inject(ViewContainerRef);
  private viewRef?: EmbeddedViewRef<any>;

  isOpen = signal(false);
  timeHour = signal(12);
  timeMinute = signal(0);
  timeAmPm = signal<'AM' | 'PM'>('AM');

  hourList = Array.from({ length: 12 }, (_, i) => i + 1);
  minuteList = Array.from({ length: 60 }, (_, i) => i);

  private wheelInited = false;
  private readonly ITEM_H = 44;

  constructor() {
    effect(() => {
      const open = this.isOpen();
      queueMicrotask(() => this.syncPortal(open));
    });
  }

  private syncPortal(open: boolean): void {
    if (open && !this.viewRef && this.modalTpl) {
      this.viewRef = this.vcr.createEmbeddedView(this.modalTpl);
      this.viewRef.detectChanges();
      this.viewRef.rootNodes.forEach(n => {
        if (n instanceof HTMLElement) document.body.appendChild(n);
      });
    } else if (!open && this.viewRef) {
      this.viewRef.destroy();
      this.viewRef = undefined;
      this.wheelInited = false;
    }
  }

  ngAfterViewChecked(): void {
    if (!this.isOpen() || !this.viewRef) { return; }
    if (this.wheelInited) return;
    const root = this.viewRef.rootNodes[0] as HTMLElement | undefined;
    if (!root) return;
    const cols = root.querySelectorAll<HTMLElement>('.wheel-col');
    if (cols.length < 2) return;
    this.wheelInited = true;
    setTimeout(() => {
      cols[0].scrollTop = (this.timeHour() - 1) * this.ITEM_H;
      cols[1].scrollTop = this.timeMinute() * this.ITEM_H;
    }, 30);
  }

  ngOnDestroy(): void { this.viewRef?.destroy(); }

  displayLabel(): string {
    if (!this.value || !/^\d{1,2}:\d{2}/.test(this.value)) return '';
    const [hStr, mStr] = this.value.split(':');
    let h = parseInt(hStr, 10);
    const m = parseInt(mStr, 10);
    const ampm = h >= 12 ? 'PM' : 'AM';
    let h12 = h % 12; if (h12 === 0) h12 = 12;
    return `${h12}:${m.toString().padStart(2, '0')} ${ampm}`;
  }

  open(ev?: Event): void {
    ev?.stopPropagation();
    let h24 = 8, m = 0;
    if (this.value && /^\d{1,2}:\d{2}/.test(this.value)) {
      const [hh, mm] = this.value.split(':').map(s => parseInt(s, 10) || 0);
      h24 = hh; m = mm;
    }
    let h12 = h24 % 12; if (h12 === 0) h12 = 12;
    this.timeHour.set(h12);
    this.timeMinute.set(m);
    this.timeAmPm.set(h24 >= 12 ? 'PM' : 'AM');
    this.isOpen.set(true);
  }

  close(): void { this.isOpen.set(false); }

  setAmPm(v: 'AM' | 'PM'): void { this.timeAmPm.set(v); }

  onWheelScroll(which: 'h' | 'm', ev: Event): void {
    const el = ev.target as HTMLElement;
    const idx = Math.round(el.scrollTop / this.ITEM_H);
    if (which === 'h') this.timeHour.set(Math.max(1, Math.min(12, idx + 1)));
    else this.timeMinute.set(Math.max(0, Math.min(59, idx)));
  }

  pickWheel(which: 'h' | 'm', val: number, ev: Event): void {
    if (which === 'h') this.timeHour.set(val);
    else this.timeMinute.set(val);
    const el = (ev.currentTarget as HTMLElement).parentElement as HTMLElement;
    const idx = which === 'h' ? val - 1 : val;
    el.scrollTop = idx * this.ITEM_H;
  }

  confirm(): void {
    let h = this.timeHour();
    const isPm = this.timeAmPm() === 'PM';
    if (isPm && h < 12) h += 12;
    if (!isPm && h === 12) h = 0;
    const hh = h.toString().padStart(2, '0');
    const mm = this.timeMinute().toString().padStart(2, '0');
    this.valueChange.emit(`${hh}:${mm}`);
    this.close();
  }
}

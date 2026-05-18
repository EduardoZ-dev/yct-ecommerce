import {
  Component, EventEmitter, Input, Output, signal, computed,
  TemplateRef, ViewChild, ViewContainerRef, EmbeddedViewRef,
  effect, inject, OnDestroy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../../components/icon/icon.component';

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <input type="text" readonly class="picker-input" [class.row-picker]="variant === 'row'"
           [value]="displayLabel()"
           (click)="open($event)"
           [placeholder]="placeholder">

    <ng-template #modalTpl>
      <div class="picker-backdrop" (click)="close()">
        <div class="picker-modal" (click)="$event.stopPropagation()">
          <header class="picker-head">
            <div>
              <small>Selecciona fecha</small>
              <h3>{{ title || label }}</h3>
            </div>
            <button class="close-btn" (click)="close()" aria-label="Cerrar">
              <app-icon name="close" [size]="20"/>
            </button>
          </header>

          <div class="picker-body">
            <div class="strip-cal">
              <header class="strip-head">
                <button class="strip-nav" (click)="prevWeek()" aria-label="Semana anterior">
                  <span class="chev left">‹</span>
                </button>
                <h2 class="strip-month">{{ monthLabel() }}</h2>
                <button class="strip-nav" (click)="nextWeek()" aria-label="Semana siguiente">
                  <span class="chev right">›</span>
                </button>
              </header>

              <div class="strip-weekdays">
                <span>D</span><span>L</span><span>M</span><span>M</span><span>J</span><span>V</span><span>S</span>
              </div>

              @for (week of weekRows(); track $index) {
                <div class="strip-week">
                  @for (day of week; track day.getTime()) {
                    <button class="strip-day"
                            [class.today]="isToday(day)"
                            [class.selected]="isSelected(day)"
                            [class.other]="isOtherMonth(day)"
                            (click)="selectDay(day)">
                      {{ day.getDate() }}
                    </button>
                  }
                </div>
              }

              <button class="strip-today-btn" (click)="goToday()">Hoy</button>
            </div>
          </div>
        </div>
      </div>
    </ng-template>
  `
})
export class DatePickerComponent implements OnDestroy {
  @Input() value: string = '';
  @Input() label = '';
  @Input() title = '';
  @Input() placeholder = 'Selecciona fecha';
  @Input() format: 'long' | 'short' = 'long';
  @Input() variant: 'field' | 'row' = 'field';
  @Output() valueChange = new EventEmitter<string>();

  @ViewChild('modalTpl') modalTpl!: TemplateRef<any>;
  private vcr = inject(ViewContainerRef);
  private viewRef?: EmbeddedViewRef<any>;

  isOpen = signal(false);
  weekBase = signal<Date>(this.startWeekMinusOne(new Date()));

  visibleDays = computed<Date[]>(() => {
    const start = this.weekBase();
    const days: Date[] = [];
    for (let i = 0; i < 21; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      days.push(d);
    }
    return days;
  });

  weekRows = computed<Date[][]>(() => {
    const d = this.visibleDays();
    return [d.slice(0, 7), d.slice(7, 14), d.slice(14, 21)];
  });

  monthLabel = computed<string>(() => {
    const middle = this.visibleDays()[10];
    const txt = middle.toLocaleDateString('es-CO', { month: 'long' });
    return txt.charAt(0).toUpperCase() + txt.slice(1);
  });

  displayLabel(): string {
    if (!this.value) return '';
    const d = new Date(this.value + 'T00:00:00');
    if (this.format === 'short') {
      return d.toLocaleDateString('es-CO', { day: '2-digit', month: 'short' });
    }
    return d.toLocaleDateString('es-CO', { day: '2-digit', month: 'long', year: 'numeric' });
  }

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
    }
  }

  ngOnDestroy(): void { this.viewRef?.destroy(); }

  open(ev?: Event): void {
    ev?.stopPropagation();
    const base = this.value ? new Date(this.value + 'T00:00:00') : new Date();
    this.weekBase.set(this.startWeekMinusOne(base));
    this.isOpen.set(true);
  }

  close(): void { this.isOpen.set(false); }

  prevWeek(): void {
    this.weekBase.update(d => { const n = new Date(d); n.setDate(n.getDate() - 7); return n; });
  }
  nextWeek(): void {
    this.weekBase.update(d => { const n = new Date(d); n.setDate(n.getDate() + 7); return n; });
  }
  goToday(): void {
    this.weekBase.set(this.startWeekMinusOne(new Date()));
  }

  selectDay(d: Date): void {
    const iso = `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`;
    this.valueChange.emit(iso);
    this.close();
  }

  isToday(d: Date): boolean {
    const t = new Date(); t.setHours(0, 0, 0, 0);
    const dd = new Date(d); dd.setHours(0, 0, 0, 0);
    return dd.getTime() === t.getTime();
  }

  isSelected(d: Date): boolean {
    if (!this.value) return false;
    const iso = `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`;
    return iso === this.value;
  }

  isOtherMonth(d: Date): boolean {
    const middle = this.visibleDays()[10];
    return d.getMonth() !== middle.getMonth();
  }

  private startWeekMinusOne(date: Date): Date {
    const d = new Date(date);
    d.setHours(0, 0, 0, 0);
    const dow = d.getDay();
    d.setDate(d.getDate() - dow - 7);
    return d;
  }
}

import {
  Component, EventEmitter, Input, Output, signal, computed,
  TemplateRef, ViewChild, ViewContainerRef, EmbeddedViewRef,
  effect, inject, OnDestroy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IconComponent } from '../../components/icon/icon.component';

export interface SelectOption<T = any> {
  value: T;
  label: string;
  sub?: string;
}

@Component({
  selector: 'app-select-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  template: `
    <input type="text" readonly class="picker-input" [class.row-picker]="variant === 'row'"
           [value]="displayLabel()"
           (click)="open($event)"
           [placeholder]="placeholder">

    <ng-template #modalTpl>
      <div class="picker-backdrop" (click)="close()">
        <div class="picker-modal select-modal" (click)="$event.stopPropagation()">
          <header class="picker-head">
            <div>
              <small>Selecciona una opción</small>
              <h3>{{ title || label }}</h3>
            </div>
            <button class="close-btn" (click)="close()" aria-label="Cerrar">
              <app-icon name="close" [size]="20"/>
            </button>
          </header>

          @if (searchable && options.length > 5) {
            <div class="select-search">
              <app-icon name="search" [size]="16"/>
              <input type="text"
                     [ngModel]="search()"
                     (ngModelChange)="search.set($event)"
                     placeholder="Buscar…"
                     autofocus>
            </div>
          }

          <div class="select-list">
            @for (opt of filtered(); track opt.value) {
              <button type="button"
                      class="select-item"
                      [class.active]="opt.value === value"
                      (click)="pick(opt.value)">
                <div class="item-label">{{ opt.label }}</div>
                @if (opt.sub) { <div class="item-sub">{{ opt.sub }}</div> }
                @if (opt.value === value) {
                  <app-icon name="check" [size]="16" class="item-check"/>
                }
              </button>
            }
            @if (filtered().length === 0) {
              <div class="select-empty">Sin resultados</div>
            }
          </div>
        </div>
      </div>
    </ng-template>
  `
})
export class SelectPickerComponent implements OnDestroy {
  @Input() value: any = null;
  @Input() options: SelectOption[] = [];
  @Input() label = '';
  @Input() title = '';
  @Input() placeholder = 'Seleccionar';
  @Input() searchable = true;
  @Input() variant: 'field' | 'row' = 'field';
  @Output() valueChange = new EventEmitter<any>();

  @ViewChild('modalTpl') modalTpl!: TemplateRef<any>;
  private vcr = inject(ViewContainerRef);
  private viewRef?: EmbeddedViewRef<any>;

  isOpen = signal(false);
  search = signal('');

  displayLabel(): string {
    const opt = this.options.find(o => o.value === this.value);
    return opt ? opt.label : '';
  }

  filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.options;
    return this.options.filter(o =>
      o.label.toLowerCase().includes(q) || (o.sub ?? '').toLowerCase().includes(q)
    );
  });

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

  ngOnDestroy(): void {
    this.viewRef?.destroy();
  }

  open(ev?: Event): void {
    ev?.stopPropagation();
    this.search.set('');
    this.isOpen.set(true);
  }

  close(): void { this.isOpen.set(false); }

  pick(value: any): void {
    this.valueChange.emit(value);
    this.close();
  }
}

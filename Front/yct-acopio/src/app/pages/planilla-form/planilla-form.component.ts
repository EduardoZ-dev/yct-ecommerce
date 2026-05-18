import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AcopioService } from '../../core/services/acopio.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toast/toast.service';
import {
  CamionDto, ConductorDto, AsistenteDto, GranjeroDto,
  SavePlanillaRequest, SavePlanillaItemRequest
} from '../../core/models';
import { IconComponent } from '../../components/icon/icon.component';
import { DatePickerComponent, TimePickerComponent, SelectPickerComponent, SelectOption } from '../../shared/pickers';

import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx-js-style';

interface RowVm {
  id?: number | null;
  fecha: string;
  granjeroId: number | null;
  cantinas: number;
  saldoLitros: number;
}

@Component({
  selector: 'app-planilla-form',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule, IconComponent,
    DatePickerComponent, TimePickerComponent, SelectPickerComponent
  ],
  templateUrl: './planilla-form.component.html',
  styleUrl: './planilla-form.component.scss'
})
export class PlanillaFormComponent implements OnInit {
  private svc = inject(AcopioService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading = signal(true);
  saving = signal(false);
  editingId = signal<number | null>(null);

  camiones = signal<CamionDto[]>([]);
  conductores = signal<ConductorDto[]>([]);
  asistentes = signal<AsistenteDto[]>([]);
  granjeros = signal<GranjeroDto[]>([]);

  // Header
  codigo = '';
  fecha = this.todayIso();
  camionId: number | null = null;
  conductorId: number | null = null;
  asistenteId: number | null = null;
  horaSalida = '';
  horaLlegadaPlanta = '';
  horaDescargue = '';
  observaciones = '';

  // Rows
  rows = signal<RowVm[]>([]);

  totalLitros = computed(() => this.rows().reduce((s, r) => s + this.calcRow(r), 0));
  totalCantinas = computed(() => this.rows().reduce((s, r) => s + (Number(r.cantinas) || 0), 0));

  showWa = signal(false);
  showEmail = signal(false);
  waPhone = '';
  emailTo = '';
  sending = signal(false);

  // Numpad (sigue inline; refactor futuro a componente)
  numPadOpen = signal<{
    title: string;
    value: string;
    isDecimal: boolean;
    max?: number;
    setter: (v: number) => void;
  } | null>(null);

  // ===== Options para selects =====
  camionOptions = computed<SelectOption<number | null>[]>(() =>
    this.camiones().map(c => ({ value: c.id, label: c.nombre, sub: c.placa ?? undefined }))
  );
  conductorOptions = computed<SelectOption<number | null>[]>(() =>
    this.conductores().map(c => ({ value: c.id, label: c.nombreCompleto, sub: c.telefono ?? undefined }))
  );
  asistenteOptions = computed<SelectOption<number | null>[]>(() => [
    { value: null, label: '— Sin asistente —' },
    ...this.asistentes().map(a => ({ value: a.id as number | null, label: a.nombreCompleto, sub: a.telefono ?? undefined }))
  ]);
  granjeroOptions = computed<SelectOption<number | null>[]>(() =>
    this.granjeros().map(g => ({ value: g.id, label: g.nombreCompleto, sub: `#${g.numero}` }))
  );

  // ===== Setters =====
  setFecha = (v: string) => { this.fecha = v; };
  setHoraLlegada = (v: string) => { this.horaLlegadaPlanta = v; };
  setHoraDescargue = (v: string) => { this.horaDescargue = v; };
  setCamionId = (v: number | null) => { this.camionId = v; this.onCamionChange(); };
  setConductorId = (v: number | null) => { this.conductorId = v; };
  setAsistenteId = (v: number | null) => { this.asistenteId = v; };

  setRowFecha(i: number): (v: string) => void {
    return (v: string) => {
      this.rows.update(list => { const next = [...list]; next[i] = { ...next[i], fecha: v }; return next; });
    };
  }
  setRowGranjero(i: number): (v: number | null) => void {
    return (v: number | null) => {
      this.rows.update(list => { const next = [...list]; next[i] = { ...next[i], granjeroId: v }; return next; });
    };
  }

  // ===== Numpad =====
  openNumPad(title: string, current: number, isDecimal: boolean, max: number | undefined, setter: (v: number) => void, ev?: Event): void {
    ev?.stopPropagation();
    this.numPadOpen.set({
      title,
      value: current && current > 0 ? String(current) : '',
      isDecimal, max, setter
    });
  }

  numPadAppend(ch: string): void {
    const open = this.numPadOpen();
    if (!open) return;
    let next = open.value;
    if (ch === '.') {
      if (!open.isDecimal) return;
      if (next.includes('.')) return;
      if (!next) next = '0';
      next += '.';
    } else {
      if (open.isDecimal && next.includes('.')) {
        const dec = next.split('.')[1] || '';
        if (dec.length >= 2) return;
      }
      next += ch;
    }
    if (open.max !== undefined) {
      const num = parseFloat(next);
      if (!isNaN(num) && num > open.max) return;
    }
    this.numPadOpen.set({ ...open, value: next });
  }

  numPadBack(): void {
    const open = this.numPadOpen();
    if (!open) return;
    this.numPadOpen.set({ ...open, value: open.value.slice(0, -1) });
  }
  numPadClear(): void {
    const open = this.numPadOpen();
    if (!open) return;
    this.numPadOpen.set({ ...open, value: '' });
  }
  numPadConfirm(): void {
    const open = this.numPadOpen();
    if (!open) return;
    const n = parseFloat(open.value) || 0;
    open.setter(n);
    this.numPadOpen.set(null);
  }
  numPadCancel(): void { this.numPadOpen.set(null); }

  setRowCantinas(i: number): (v: number) => void {
    return (v: number) => {
      this.rows.update(list => { const next = [...list]; next[i] = { ...next[i], cantinas: Math.max(0, Math.floor(v)) }; return next; });
    };
  }
  setRowSaldo(i: number): (v: number) => void {
    return (v: number) => {
      this.rows.update(list => {
        const next = [...list];
        let val = v;
        if (val < 0) val = 0;
        if (val >= 40) val = 39.99;
        next[i] = { ...next[i], saldoLitros: Math.round(val * 100) / 100 };
        return next;
      });
    };
  }

  // ===== Stepper =====
  incCantina(i: number, delta: number): void {
    this.rows.update(list => {
      const next = [...list];
      const v = (Number(next[i].cantinas) || 0) + delta;
      next[i] = { ...next[i], cantinas: Math.max(0, v) };
      return next;
    });
  }
  incSaldo(i: number, delta: number): void {
    this.rows.update(list => {
      const next = [...list];
      let v = (Number(next[i].saldoLitros) || 0) + delta;
      if (v < 0) v = 0;
      if (v >= 40) v = 39.5;
      next[i] = { ...next[i], saldoLitros: Math.round(v * 100) / 100 };
      return next;
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'nueva') this.editingId.set(+id);

    forkJoin({
      cam: this.svc.getCamiones(),
      cond: this.svc.getConductores(),
      asis: this.svc.getAsistentes(),
      gran: this.svc.getGranjeros()
    }).subscribe({
      next: ({ cam, cond, asis, gran }) => {
        this.camiones.set((cam.data ?? []).filter(c => c.estado === 'Activo'));
        this.conductores.set((cond.data ?? []).filter(c => c.isActive));
        this.asistentes.set((asis.data ?? []).filter(a => a.isActive));
        this.granjeros.set((gran.data ?? []).filter(g => g.isActive));

        const id = this.editingId();
        if (id) this.loadPlanilla(id);
        else { this.loading.set(false); this.addRow(); }
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Error cargando catálogos');
      }
    });
  }

  private loadPlanilla(id: number): void {
    this.svc.getPlanilla(id).subscribe({
      next: (res) => {
        if (!res.success || !res.data) { this.toast.error(res.message || 'Planilla no encontrada'); this.loading.set(false); return; }
        const p = res.data;
        this.codigo = p.codigo;
        this.fecha = (p.fecha || '').substring(0, 10);
        this.camionId = p.camionId;
        this.conductorId = p.conductorId;
        this.asistenteId = p.asistenteId ?? null;
        this.horaSalida = this.timeToInput(p.horaSalida);
        this.horaLlegadaPlanta = this.timeToInput(p.horaLlegadaPlanta);
        this.horaDescargue = this.timeToInput(p.horaDescargue);
        this.observaciones = p.observaciones ?? '';
        this.rows.set((p.items ?? []).map(it => ({
          id: it.id ?? null,
          fecha: (it.fecha || p.fecha).substring(0, 10),
          granjeroId: it.granjeroId,
          cantinas: it.cantinas,
          saldoLitros: it.saldoLitros
        })));
        if (this.rows().length === 0) this.addRow();
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.toast.error('Error cargando planilla'); }
    });
  }

  // ===== Rows =====
  addRow(): void {
    this.rows.update(list => [...list, {
      fecha: this.fecha, granjeroId: null, cantinas: 0, saldoLitros: 0
    }]);
  }

  removeRow(i: number): void {
    this.rows.update(list => list.filter((_, idx) => idx !== i));
    if (this.rows().length === 0) this.addRow();
  }

  calcRow(r: RowVm): number {
    return (Number(r.cantinas) || 0) * 40 + (Number(r.saldoLitros) || 0);
  }

  onCamionChange(): void {
    const c = this.camiones().find(x => x.id === this.camionId);
    if (c && !this.codigo) this.codigo = c.nombre.toUpperCase();
  }

  // ===== Save =====
  save(): void {
    if (!this.validate()) return;
    this.saving.set(true);

    const items: SavePlanillaItemRequest[] = this.rows()
      .filter(r => r.granjeroId)
      .map(r => ({
        id: r.id ?? null,
        granjeroId: r.granjeroId!,
        fecha: r.fecha,
        cantinas: Number(r.cantinas) || 0,
        saldoLitros: Number(r.saldoLitros) || 0
      }));

    const payload: SavePlanillaRequest = {
      id: this.editingId(),
      codigo: this.codigo.trim().toUpperCase(),
      fecha: this.fecha,
      camionId: this.camionId!,
      conductorId: this.conductorId!,
      asistenteId: this.asistenteId,
      horaSalida: this.inputToTime(this.horaSalida),
      horaLlegadaPlanta: this.inputToTime(this.horaLlegadaPlanta),
      horaDescargue: this.inputToTime(this.horaDescargue),
      observaciones: this.observaciones.trim() || null,
      items
    };

    const id = this.editingId();
    const req = id ? this.svc.updatePlanilla(id, payload) : this.svc.createPlanilla(payload);

    req.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success(`Planilla guardada · ${this.totalLitros().toFixed(0)} L`);
          this.router.navigate(['/planillas']);
        } else this.toast.error(res.message);
      },
      error: (e) => {
        this.saving.set(false);
        this.toast.error(e.error?.message ?? 'Error al guardar');
      }
    });
  }

  private validate(): boolean {
    if (!this.codigo.trim()) { this.toast.warning('El código de ruta es obligatorio'); return false; }
    if (!this.fecha) { this.toast.warning('La fecha es obligatoria'); return false; }
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const f = new Date(this.fecha + 'T00:00:00');
    if (f.getTime() > today.getTime()) { this.toast.warning('La fecha de la planilla no puede ser futura'); return false; }
    if (!this.camionId) { this.toast.warning('Selecciona un camión'); return false; }
    if (!this.conductorId) { this.toast.warning('Selecciona un conductor'); return false; }

    const tSal = this.timeMinutes(this.horaSalida);
    const tLle = this.timeMinutes(this.horaLlegadaPlanta);
    const tDes = this.timeMinutes(this.horaDescargue);
    if (tSal != null && tLle != null && tLle < tSal) { this.toast.warning('La hora de llegada no puede ser anterior a la hora de salida'); return false; }
    if (tLle != null && tDes != null && tDes < tLle) { this.toast.warning('La hora de descargue no puede ser anterior a la hora de llegada'); return false; }
    if (tSal != null && tDes != null && tDes < tSal) { this.toast.warning('La hora de descargue no puede ser anterior a la hora de salida'); return false; }

    const validRows = this.rows().filter(r => r.granjeroId);
    if (validRows.length === 0) { this.toast.warning('Agrega al menos un granjero a la planilla'); return false; }

    const ids = validRows.map(r => r.granjeroId!);
    const dup = ids.find((id, i) => ids.indexOf(id) !== i);
    if (dup) {
      const g = this.granjeros().find(x => x.id === dup);
      this.toast.warning(`Granjero "${g?.nombreCompleto ?? dup}" está repetido`); return false;
    }

    for (const r of validRows) {
      if (r.saldoLitros >= 40) { this.toast.warning(`Saldo debe ser menor a 40 (si son 40 completos suma 1 cantina)`); return false; }
      if (r.saldoLitros < 0 || r.cantinas < 0) { this.toast.warning('Valores no pueden ser negativos'); return false; }
      if (r.cantinas === 0 && r.saldoLitros === 0) {
        const g = this.granjeros().find(x => x.id === r.granjeroId);
        this.toast.warning(`Falta ingresar litros del granjero "${g?.nombreCompleto ?? r.granjeroId}"`); return false;
      }
      if (r.fecha) {
        const rf = new Date(r.fecha + 'T00:00:00');
        if (rf.getTime() > today.getTime()) { this.toast.warning('Una fecha de recogida está en el futuro'); return false; }
      }
    }
    return true;
  }

  private timeMinutes(t: string): number | null {
    if (!t || !/^\d{1,2}:\d{2}/.test(t)) return null;
    const [h, m] = t.split(':').map(s => parseInt(s, 10) || 0);
    return h * 60 + m;
  }

  // ===== Exports =====
  exportPdf(open = true): Blob | null {
    const data = this.buildExportData();
    if (!data) return null;

    const doc = new jsPDF({ unit: 'mm', format: 'a4' });
    const margin = 15;
    let y = margin;

    doc.setFontSize(16);
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(90, 158, 47);
    doc.text('YCT DISTRIBUCIONES', margin, y);
    doc.setFontSize(9);
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(122, 138, 122);
    y += 5;
    doc.text('Derivados Lácteos · NIT 900.955.788-8 · Régimen Común', margin, y);
    y += 4;
    doc.text('Cra. 5 No. 5A-36 San Diego · Cesar · Cel. 3103625290', margin, y);
    y += 4;
    doc.text('distribucionesyctsas@gmail.com', margin, y);

    y += 10;
    doc.setFontSize(13);
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(30, 42, 30);
    doc.text('PLANILLA DE ACOPIO LECHERO', margin, y);

    y += 7;
    doc.setFontSize(10);
    doc.setFont('helvetica', 'normal');
    const colW = (210 - margin * 2) / 2;
    const labelV = (label: string, value: string, x: number, yy: number) => {
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(122, 138, 122);
      doc.text(label.toUpperCase(), x, yy);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(30, 42, 30);
      doc.text(value || '—', x, yy + 4);
    };
    labelV('Ruta', data.codigo, margin, y);
    labelV('Código transportador', data.camionPlaca || '—', margin + colW, y);
    y += 10;
    labelV('Fecha', this.formatDate(data.fecha), margin, y);
    labelV('Conductor', data.conductorNombre, margin + colW, y);
    y += 10;
    labelV('Hora llegada', data.horaLlegadaPlanta || '—', margin, y);
    labelV('Hora descargue', data.horaDescargue || '—', margin + colW, y);

    if (data.asistenteNombre) {
      y += 10;
      labelV('Asistente', data.asistenteNombre, margin, y);
    }

    y += 8;

    autoTable(doc, {
      startY: y,
      head: [['#', 'Fecha', 'No. Proveedor', 'Cantinas (40 L)', 'Saldo Lts', 'Total Litros']],
      body: data.items.map((r, i) => [
        (i + 1).toString(),
        this.formatDateShort(r.fecha),
        `#${r.granjeroNumero}  ${r.granjeroNombre}`,
        r.cantinas.toString(),
        r.saldoLitros.toFixed(2),
        r.totalLitros.toFixed(2)
      ]),
      foot: [['', '', '', data.totalCantinas.toString(), '', data.totalLitros.toFixed(2) + ' L']],
      theme: 'grid',
      headStyles: { fillColor: [90, 158, 47], textColor: 255, fontStyle: 'bold', fontSize: 9 },
      footStyles: { fillColor: [237, 247, 229], textColor: [30, 42, 30], fontStyle: 'bold' },
      bodyStyles: { fontSize: 9, textColor: [30, 42, 30] },
      columnStyles: {
        0: { halign: 'center', cellWidth: 10 },
        1: { halign: 'center', cellWidth: 22 },
        3: { halign: 'right', cellWidth: 25 },
        4: { halign: 'right', cellWidth: 22 },
        5: { halign: 'right', cellWidth: 28 }
      },
      margin: { left: margin, right: margin }
    });

    let endY = (doc as any).lastAutoTable.finalY + 8;

    if (data.observaciones) {
      doc.setFontSize(9);
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(122, 138, 122);
      doc.text('OBSERVACIONES', margin, endY);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(30, 42, 30);
      doc.text(data.observaciones, margin, endY + 5, { maxWidth: 210 - margin * 2 });
      endY += 14;
    }

    endY = Math.max(endY, 240);
    const signW = (210 - margin * 2 - 10) / 2;
    doc.setDrawColor(30, 42, 30);
    doc.line(margin, endY + 15, margin + signW, endY + 15);
    doc.line(margin + signW + 10, endY + 15, margin + signW + 10 + signW, endY + 15);
    doc.setFontSize(8);
    doc.setTextColor(122, 138, 122);
    doc.text('Firma chofer', margin + signW / 2, endY + 19, { align: 'center' });
    doc.text('Firma recibido planta', margin + signW + 10 + signW / 2, endY + 19, { align: 'center' });

    doc.setFontSize(7);
    doc.setTextColor(140, 140, 140);
    doc.text(`Generado: ${new Date().toLocaleString('es-CO')} · ${this.auth.fullName()}`, margin, 290);

    const fileName = `planilla-${data.codigo}-${data.fecha.replace(/-/g, '')}.pdf`;
    const blob = doc.output('blob');

    if (open) doc.save(fileName);
    return blob;
  }

  exportExcel(): void {
    const data = this.buildExportData();
    if (!data) return;

    const wb = XLSX.utils.book_new();

    const COLOR_BRAND = '5A9E2F';
    const COLOR_TEXT = '1E2A1E';
    const COLOR_MUTED = '7A8A7A';
    const COLOR_LIGHT_BG = 'EDF7E5';
    const BORDER_GRAY = { style: 'thin' as const, color: { rgb: 'D0D7CC' } };

    const stTitle = {
      font: { name: 'Calibri', sz: 16, bold: true, color: { rgb: COLOR_BRAND } },
      alignment: { horizontal: 'left' as const, vertical: 'center' as const }
    };
    const stSubtitle = {
      font: { name: 'Calibri', sz: 9, italic: true, color: { rgb: COLOR_MUTED } },
      alignment: { horizontal: 'left' as const }
    };
    const stLabel = {
      font: { name: 'Calibri', sz: 9, bold: true, color: { rgb: COLOR_MUTED } },
      alignment: { horizontal: 'left' as const, vertical: 'center' as const }
    };
    const stValue = {
      font: { name: 'Calibri', sz: 11, color: { rgb: COLOR_TEXT } },
      alignment: { horizontal: 'left' as const, vertical: 'center' as const }
    };
    const stTblHead = {
      font: { name: 'Calibri', sz: 10, bold: true, color: { rgb: 'FFFFFF' } },
      fill: { fgColor: { rgb: COLOR_BRAND } },
      alignment: { horizontal: 'center' as const, vertical: 'center' as const },
      border: { top: BORDER_GRAY, bottom: BORDER_GRAY, left: BORDER_GRAY, right: BORDER_GRAY }
    };
    const stTblCell = {
      font: { name: 'Calibri', sz: 10, color: { rgb: COLOR_TEXT } },
      alignment: { vertical: 'center' as const },
      border: { top: BORDER_GRAY, bottom: BORDER_GRAY, left: BORDER_GRAY, right: BORDER_GRAY }
    };
    const stTblCellNum = { ...stTblCell, alignment: { horizontal: 'right' as const, vertical: 'center' as const } };
    const stTblCellCenter = { ...stTblCell, alignment: { horizontal: 'center' as const, vertical: 'center' as const } };
    const stTotalsLabel = {
      font: { name: 'Calibri', sz: 11, bold: true, color: { rgb: COLOR_TEXT } },
      fill: { fgColor: { rgb: COLOR_LIGHT_BG } },
      alignment: { horizontal: 'right' as const, vertical: 'center' as const },
      border: { top: BORDER_GRAY, bottom: BORDER_GRAY, left: BORDER_GRAY, right: BORDER_GRAY }
    };
    const stTotalsValue = {
      font: { name: 'Calibri', sz: 11, bold: true, color: { rgb: COLOR_BRAND } },
      fill: { fgColor: { rgb: COLOR_LIGHT_BG } },
      alignment: { horizontal: 'right' as const, vertical: 'center' as const },
      border: { top: BORDER_GRAY, bottom: BORDER_GRAY, left: BORDER_GRAY, right: BORDER_GRAY }
    };
    const stObsLabel = { font: { name: 'Calibri', sz: 9, bold: true, color: { rgb: COLOR_MUTED } } };
    const stObsValue = {
      font: { name: 'Calibri', sz: 10, color: { rgb: COLOR_TEXT } },
      alignment: { wrapText: true, vertical: 'top' as const }
    };

    const ws: any = {};
    const set = (addr: string, value: any, style?: any) => {
      const t = typeof value === 'number' ? 'n' : 's';
      ws[addr] = { v: value, t, s: style };
    };

    set('A1', 'YCT DISTRIBUCIONES', stTitle);
    set('A2', 'Derivados Lácteos · NIT 900.955.788-8 · Cra. 5 No. 5A-36 San Diego, Cesar', stSubtitle);
    set('A3', 'Planilla de acopio lechero', { font: { sz: 12, bold: true, color: { rgb: COLOR_TEXT } } });

    set('A5', 'RUTA', stLabel);             set('B5', data.codigo, stValue);
    set('D5', 'FECHA', stLabel);            set('E5', this.formatDate(data.fecha), stValue);
    set('A6', 'CAMIÓN', stLabel);           set('B6', `${data.camionNombre}${data.camionPlaca ? ' (' + data.camionPlaca + ')' : ''}`, stValue);
    set('D6', 'CONDUCTOR', stLabel);        set('E6', data.conductorNombre, stValue);
    set('A7', 'ASISTENTE', stLabel);        set('B7', data.asistenteNombre || '—', stValue);
    set('D7', 'HORA LLEGADA', stLabel);     set('E7', data.horaLlegadaPlanta || '—', stValue);
    set('A8', 'HORA DESCARGUE', stLabel);   set('B8', data.horaDescargue || '—', stValue);

    const tableHead = ['#', 'Fecha', 'No. Proveedor', 'Nombre', 'Cantinas (40 L)', 'Saldo Lts', 'Total Litros'];
    const headRow = 10;
    tableHead.forEach((h, i) => {
      set(XLSX.utils.encode_cell({ r: headRow - 1, c: i }), h, stTblHead);
    });

    data.items.forEach((r, i) => {
      const rowIdx = headRow + i;
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 0 }), i + 1, stTblCellCenter);
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 1 }), this.formatDateShort(r.fecha), stTblCellCenter);
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 2 }), r.granjeroNumero, stTblCellCenter);
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 3 }), r.granjeroNombre, stTblCell);
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 4 }), r.cantinas, stTblCellNum);
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 5 }), r.saldoLitros, stTblCellNum);
      set(XLSX.utils.encode_cell({ r: rowIdx, c: 6 }), r.totalLitros, stTblCellNum);
    });

    const totalsRowIdx = headRow + data.items.length;
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 0 }), '', stTotalsLabel);
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 1 }), '', stTotalsLabel);
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 2 }), '', stTotalsLabel);
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 3 }), 'TOTAL', stTotalsLabel);
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 4 }), data.totalCantinas, stTotalsValue);
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 5 }), '', stTotalsLabel);
    set(XLSX.utils.encode_cell({ r: totalsRowIdx, c: 6 }), data.totalLitros, stTotalsValue);

    let lastRow = totalsRowIdx;
    if (data.observaciones) {
      const obsRow = totalsRowIdx + 2;
      set(XLSX.utils.encode_cell({ r: obsRow, c: 0 }), 'OBSERVACIONES', stObsLabel);
      set(XLSX.utils.encode_cell({ r: obsRow + 1, c: 0 }), data.observaciones, stObsValue);
      lastRow = obsRow + 1;
    }

    ws['!merges'] = [
      { s: { r: 0, c: 0 }, e: { r: 0, c: 6 } },
      { s: { r: 1, c: 0 }, e: { r: 1, c: 6 } },
      { s: { r: 2, c: 0 }, e: { r: 2, c: 6 } },
      { s: { r: 4, c: 1 }, e: { r: 4, c: 2 } },
      { s: { r: 4, c: 4 }, e: { r: 4, c: 6 } },
      { s: { r: 5, c: 1 }, e: { r: 5, c: 2 } },
      { s: { r: 5, c: 4 }, e: { r: 5, c: 6 } },
      { s: { r: 6, c: 1 }, e: { r: 6, c: 2 } },
      { s: { r: 6, c: 4 }, e: { r: 6, c: 6 } },
      { s: { r: 7, c: 1 }, e: { r: 7, c: 2 } },
    ];
    if (data.observaciones) {
      ws['!merges'].push({ s: { r: lastRow, c: 0 }, e: { r: lastRow, c: 6 } });
    }

    ws['!cols'] = [
      { wch: 6 }, { wch: 14 }, { wch: 14 }, { wch: 30 },
      { wch: 16 }, { wch: 12 }, { wch: 14 }
    ];
    ws['!rows'] = [
      { hpt: 24 }, { hpt: 16 }, { hpt: 18 }, { hpt: 8 },
      { hpt: 22 }, { hpt: 22 }, { hpt: 22 }, { hpt: 22 },
      { hpt: 8 }, { hpt: 24 }
    ];
    ws['!ref'] = XLSX.utils.encode_range({ s: { r: 0, c: 0 }, e: { r: lastRow + 1, c: 6 } });

    XLSX.utils.book_append_sheet(wb, ws, 'Planilla');
    XLSX.writeFile(wb, `planilla-${data.codigo}-${data.fecha.replace(/-/g, '')}.xlsx`);
  }

  openWhatsapp(): void {
    if (!this.editingId() && !this.totalLitros()) {
      this.toast.warning('Guarda la planilla antes de enviar por WhatsApp');
      return;
    }
    this.waPhone = '';
    this.showWa.set(true);
  }

  sendWhatsapp(): void {
    const phone = this.normalizePhone(this.waPhone);
    if (!phone) { this.toast.warning('Teléfono inválido'); return; }

    const data = this.buildExportData();
    if (!data) return;

    const itemsTxt = data.items
      .map((r, i) => `${i + 1}. #${r.granjeroNumero} ${r.granjeroNombre} → ${r.cantinas} cant + ${r.saldoLitros} L = *${r.totalLitros.toFixed(2)} L*`)
      .join('%0A');

    const text =
      `*Planilla acopio · ${data.codigo}*%0A` +
      `Fecha: ${this.formatDate(data.fecha)}%0A` +
      `Conductor: ${data.conductorNombre}%0A` +
      `${data.asistenteNombre ? 'Asistente: ' + data.asistenteNombre + '%0A' : ''}` +
      `%0A${itemsTxt}%0A%0A` +
      `*TOTAL: ${data.totalLitros.toFixed(2)} L · ${data.totalCantinas} cantinas*`;

    window.open(`https://wa.me/${phone}?text=${text}`, '_blank');
    this.showWa.set(false);
    this.toast.success('Abriendo WhatsApp…');
  }

  openEmail(): void {
    if (!this.editingId()) { this.toast.warning('Guarda la planilla primero para enviarla por email'); return; }
    this.emailTo = '';
    this.showEmail.set(true);
  }

  sendEmail(): void {
    const id = this.editingId();
    if (!id) return;
    if (!this.emailTo.trim() || !this.emailTo.includes('@')) { this.toast.warning('Correo destinatario inválido'); return; }

    this.sending.set(true);
    const pdfBlob = this.exportPdf(false);

    const send = (b64: string | null) => {
      this.svc.sendPlanillaEmail(id, {
        to: this.emailTo.trim(),
        pdfBase64: b64,
        pdfFileName: b64 ? `planilla-${this.codigo}-${this.fecha.replace(/-/g, '')}.pdf` : null
      }).subscribe({
        next: (res) => {
          this.sending.set(false);
          if (res.success) { this.toast.success('Correo enviado'); this.showEmail.set(false); }
          else this.toast.error(res.message);
        },
        error: (e) => { this.sending.set(false); this.toast.error(e.error?.message ?? 'Error enviando correo'); }
      });
    };

    if (!pdfBlob) { send(null); return; }
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      const b64 = result.split(',')[1] ?? null;
      send(b64);
    };
    reader.onerror = () => { this.sending.set(false); this.toast.error('Error generando PDF'); };
    reader.readAsDataURL(pdfBlob);
  }

  // ===== Helpers =====
  private buildExportData() {
    const camion = this.camiones().find(c => c.id === this.camionId);
    const conductor = this.conductores().find(c => c.id === this.conductorId);
    const asistente = this.asistenteId ? this.asistentes().find(a => a.id === this.asistenteId) : null;
    if (!camion || !conductor) { this.toast.warning('Faltan datos en la cabecera'); return null; }

    const items = this.rows()
      .filter(r => r.granjeroId)
      .map(r => {
        const g = this.granjeros().find(x => x.id === r.granjeroId)!;
        return {
          granjeroNumero: g.numero,
          granjeroNombre: g.nombreCompleto,
          fecha: r.fecha,
          cantinas: Number(r.cantinas) || 0,
          saldoLitros: Number(r.saldoLitros) || 0,
          totalLitros: this.calcRow(r)
        };
      });

    return {
      codigo: this.codigo || camion.nombre.toUpperCase(),
      fecha: this.fecha,
      camionNombre: camion.nombre,
      camionPlaca: camion.placa,
      conductorNombre: conductor.nombreCompleto,
      asistenteNombre: asistente?.nombreCompleto,
      horaLlegadaPlanta: this.horaLlegadaPlanta,
      horaDescargue: this.horaDescargue,
      observaciones: this.observaciones,
      items,
      totalCantinas: this.totalCantinas(),
      totalLitros: this.totalLitros()
    };
  }

  private todayIso(): string {
    const d = new Date();
    return `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`;
  }

  private timeToInput(ts: string | null | undefined): string {
    if (!ts) return '';
    return ts.length >= 5 ? ts.substring(0, 5) : '';
  }

  private inputToTime(s: string): string | null {
    if (!s) return null;
    return s.length === 5 ? `${s}:00` : s;
  }

  private formatDate(iso: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('es-CO', { day: '2-digit', month: 'long', year: 'numeric' });
  }

  private formatDateShort(iso: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: '2-digit' });
  }

  private normalizePhone(p: string): string {
    if (!p) return '';
    const digits = p.replace(/\D/g, '');
    if (!digits) return '';
    if (digits.startsWith('57')) return digits;
    if (digits.length === 10) return `57${digits}`;
    return digits;
  }
}

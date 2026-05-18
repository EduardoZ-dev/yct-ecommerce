// ===== Auth =====
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  fullName: string;
  role: string;
  avatarUrl?: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

// ===== Common =====
export interface ResponseBase<T> {
  success: boolean;
  message: string;
  data: T;
}

// ===== Acopio =====
export type CamionEstado = 'Activo' | 'Mantenimiento' | 'Inactivo';

export interface CamionDto {
  id: number;
  nombre: string;
  placa?: string | null;
  estado: CamionEstado;
  notas?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface SaveCamionRequest {
  id?: number | null;
  nombre: string;
  placa?: string | null;
  estado: CamionEstado;
  notas?: string | null;
}

export interface ConductorDto {
  id: number;
  nombreCompleto: string;
  cedula?: string | null;
  telefono?: string | null;
  camionPreferidoId?: number | null;
  camionPreferidoNombre?: string | null;
  userId?: number | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface SaveConductorRequest {
  id?: number | null;
  nombreCompleto: string;
  cedula?: string | null;
  telefono?: string | null;
  camionPreferidoId?: number | null;
  userId?: number | null;
  isActive: boolean;
}

export interface AsistenteDto {
  id: number;
  nombreCompleto: string;
  cedula?: string | null;
  telefono?: string | null;
  camionPreferidoId?: number | null;
  camionPreferidoNombre?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface SaveAsistenteRequest {
  id?: number | null;
  nombreCompleto: string;
  cedula?: string | null;
  telefono?: string | null;
  camionPreferidoId?: number | null;
  isActive: boolean;
}

export interface GranjeroDto {
  id: number;
  numero: number;
  nombreCompleto: string;
  cedula?: string | null;
  telefono?: string | null;
  finca?: string | null;
  vereda?: string | null;
  municipio?: string | null;
  precioLitro?: number | null;
  promedioDiario?: number | null;
  notas?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface SaveGranjeroRequest {
  id?: number | null;
  numero: number;
  nombreCompleto: string;
  cedula?: string | null;
  telefono?: string | null;
  finca?: string | null;
  vereda?: string | null;
  municipio?: string | null;
  precioLitro?: number | null;
  notas?: string | null;
  isActive: boolean;
}

// ===== Planillas =====
export interface PlanillaHeaderDto {
  id: number;
  codigo: string;
  fecha: string;
  camionId: number;
  camionNombre: string;
  conductorId: number;
  conductorNombre: string;
  asistenteId?: number | null;
  asistenteNombre?: string | null;
  horaSalida?: string | null;
  horaLlegadaPlanta?: string | null;
  horaDescargue?: string | null;
  totalLitros: number;
  totalCantinas: number;
  totalRecogidas: number;
  status: string;
  observaciones?: string | null;
  createdAt: string;
}

export interface PlanillaItemDto {
  id?: number | null;
  granjeroId: number;
  granjeroNumero: number;
  granjeroNombre: string;
  fecha: string;
  cantinas: number;
  saldoLitros: number;
  totalLitros: number;
}

export interface PlanillaDto extends PlanillaHeaderDto {
  items: PlanillaItemDto[];
}

export interface SavePlanillaItemRequest {
  id?: number | null;
  granjeroId: number;
  fecha: string;
  cantinas: number;
  saldoLitros: number;
}

export interface SavePlanillaRequest {
  id?: number | null;
  codigo: string;
  fecha: string;            // ISO yyyy-MM-dd
  camionId: number;
  conductorId: number;
  asistenteId?: number | null;
  horaSalida?: string | null;           // HH:mm:ss
  horaLlegadaPlanta?: string | null;
  horaDescargue?: string | null;
  observaciones?: string | null;
  items: SavePlanillaItemRequest[];
}

export interface SendPlanillaEmailRequest {
  to: string;
  subject?: string | null;
  body?: string | null;
  pdfBase64?: string | null;
  pdfFileName?: string | null;
}

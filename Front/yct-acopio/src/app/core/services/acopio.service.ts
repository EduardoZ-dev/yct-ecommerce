import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ResponseBase,
  CamionDto, SaveCamionRequest,
  ConductorDto, SaveConductorRequest,
  AsistenteDto, SaveAsistenteRequest,
  GranjeroDto, SaveGranjeroRequest,
  PlanillaHeaderDto, PlanillaDto, SavePlanillaRequest, SendPlanillaEmailRequest
} from '../models';

@Injectable({ providedIn: 'root' })
export class AcopioService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/acopio`;

  // ===== Camiones =====
  getCamiones(): Observable<ResponseBase<CamionDto[]>> {
    return this.http.get<ResponseBase<CamionDto[]>>(`${this.base}/Camiones`);
  }
  createCamion(body: SaveCamionRequest): Observable<ResponseBase<CamionDto>> {
    return this.http.post<ResponseBase<CamionDto>>(`${this.base}/Camiones`, body);
  }
  updateCamion(id: number, body: SaveCamionRequest): Observable<ResponseBase<CamionDto>> {
    return this.http.put<ResponseBase<CamionDto>>(`${this.base}/Camiones/${id}`, body);
  }
  deleteCamion(id: number): Observable<ResponseBase<boolean>> {
    return this.http.delete<ResponseBase<boolean>>(`${this.base}/Camiones/${id}`);
  }

  // ===== Conductores =====
  getConductores(): Observable<ResponseBase<ConductorDto[]>> {
    return this.http.get<ResponseBase<ConductorDto[]>>(`${this.base}/Conductores`);
  }
  createConductor(body: SaveConductorRequest): Observable<ResponseBase<ConductorDto>> {
    return this.http.post<ResponseBase<ConductorDto>>(`${this.base}/Conductores`, body);
  }
  updateConductor(id: number, body: SaveConductorRequest): Observable<ResponseBase<ConductorDto>> {
    return this.http.put<ResponseBase<ConductorDto>>(`${this.base}/Conductores/${id}`, body);
  }
  deleteConductor(id: number): Observable<ResponseBase<boolean>> {
    return this.http.delete<ResponseBase<boolean>>(`${this.base}/Conductores/${id}`);
  }

  // ===== Asistentes =====
  getAsistentes(): Observable<ResponseBase<AsistenteDto[]>> {
    return this.http.get<ResponseBase<AsistenteDto[]>>(`${this.base}/Asistentes`);
  }
  createAsistente(body: SaveAsistenteRequest): Observable<ResponseBase<AsistenteDto>> {
    return this.http.post<ResponseBase<AsistenteDto>>(`${this.base}/Asistentes`, body);
  }
  updateAsistente(id: number, body: SaveAsistenteRequest): Observable<ResponseBase<AsistenteDto>> {
    return this.http.put<ResponseBase<AsistenteDto>>(`${this.base}/Asistentes/${id}`, body);
  }
  deleteAsistente(id: number): Observable<ResponseBase<boolean>> {
    return this.http.delete<ResponseBase<boolean>>(`${this.base}/Asistentes/${id}`);
  }

  // ===== Granjeros =====
  getGranjeros(): Observable<ResponseBase<GranjeroDto[]>> {
    return this.http.get<ResponseBase<GranjeroDto[]>>(`${this.base}/Granjeros`);
  }
  createGranjero(body: SaveGranjeroRequest): Observable<ResponseBase<GranjeroDto>> {
    return this.http.post<ResponseBase<GranjeroDto>>(`${this.base}/Granjeros`, body);
  }
  updateGranjero(id: number, body: SaveGranjeroRequest): Observable<ResponseBase<GranjeroDto>> {
    return this.http.put<ResponseBase<GranjeroDto>>(`${this.base}/Granjeros/${id}`, body);
  }
  deleteGranjero(id: number): Observable<ResponseBase<boolean>> {
    return this.http.delete<ResponseBase<boolean>>(`${this.base}/Granjeros/${id}`);
  }

  // ===== Planillas =====
  getPlanillas(): Observable<ResponseBase<PlanillaHeaderDto[]>> {
    return this.http.get<ResponseBase<PlanillaHeaderDto[]>>(`${this.base}/Planillas`);
  }
  getPlanilla(id: number): Observable<ResponseBase<PlanillaDto>> {
    return this.http.get<ResponseBase<PlanillaDto>>(`${this.base}/Planillas/${id}`);
  }
  createPlanilla(body: SavePlanillaRequest): Observable<ResponseBase<PlanillaDto>> {
    return this.http.post<ResponseBase<PlanillaDto>>(`${this.base}/Planillas`, body);
  }
  updatePlanilla(id: number, body: SavePlanillaRequest): Observable<ResponseBase<PlanillaDto>> {
    return this.http.put<ResponseBase<PlanillaDto>>(`${this.base}/Planillas/${id}`, body);
  }
  deletePlanilla(id: number): Observable<ResponseBase<boolean>> {
    return this.http.delete<ResponseBase<boolean>>(`${this.base}/Planillas/${id}`);
  }
  sendPlanillaEmail(id: number, body: SendPlanillaEmailRequest): Observable<ResponseBase<boolean>> {
    return this.http.post<ResponseBase<boolean>>(`${this.base}/Planillas/${id}/email`, body);
  }
}

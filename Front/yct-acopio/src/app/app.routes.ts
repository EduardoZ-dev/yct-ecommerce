import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'planillas', pathMatch: 'full' },
      { path: 'planillas',         loadComponent: () => import('./pages/planillas/planillas.component').then(m => m.PlanillasComponent) },
      { path: 'planillas/nueva',   loadComponent: () => import('./pages/planilla-form/planilla-form.component').then(m => m.PlanillaFormComponent) },
      { path: 'planillas/:id',     loadComponent: () => import('./pages/planilla-form/planilla-form.component').then(m => m.PlanillaFormComponent) },
      { path: 'camiones',    loadComponent: () => import('./pages/camiones/camiones.component').then(m => m.CamionesComponent) },
      { path: 'conductores', loadComponent: () => import('./pages/conductores/conductores.component').then(m => m.ConductoresComponent) },
      { path: 'asistentes',  loadComponent: () => import('./pages/asistentes/asistentes.component').then(m => m.AsistentesComponent) },
      { path: 'granjeros',   loadComponent: () => import('./pages/granjeros/granjeros.component').then(m => m.GranjerosComponent) }
    ]
  },
  { path: '**', redirectTo: '' }
];

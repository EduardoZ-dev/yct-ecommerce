import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

const ADMIN_PANEL_ROLES = ['SuperAdmin', 'Admin', 'Employee'];

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: '/admin' } });
    return false;
  }

  if (!ADMIN_PANEL_ROLES.includes(auth.role())) {
    router.navigate(['/shop']);
    return false;
  }

  return true;
};

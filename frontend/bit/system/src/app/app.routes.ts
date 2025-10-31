import { Routes } from '@angular/router';

export const routes: Routes = [
  //modulos
  {
    path: 'index',
    loadChildren: () =>
      import('./features/index/index.route').then((r) => r.INDEX_ROUTES),
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then((r) => r.AUTH_ROUTES),
  },
  {
    path: '**',
    redirectTo: 'index',
  },
];

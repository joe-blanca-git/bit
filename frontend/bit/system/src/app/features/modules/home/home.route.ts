import { Routes } from '@angular/router';
import { HomeAppComponent } from './home.app.component';

export const HOME_ROUTES: Routes = [
  {
    path: '',
    component: HomeAppComponent,
    children: [
    ],
  },

];
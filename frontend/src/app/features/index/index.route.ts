import { Routes } from '@angular/router';
import { IndexAppComponent } from './index.app.component';
import { AuthGuardService } from '../../core/guards/auth.guard.ts.service';
import { HomeAppComponent } from '../modules/home/home.app.component';


export const INDEX_ROUTES: Routes = [
  {
    path: '',
    component: IndexAppComponent,
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
      {
        path: 'home',
        component: HomeAppComponent,
        // canActivate: [AuthGuardService]
      },
    //   {
    //     path: 'profile',
    //     component: ProfileComponent,
    //     canActivate: [AuthGuardService]
    //   },
    ],
  },
];

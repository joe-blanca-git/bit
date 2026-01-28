import { Routes } from '@angular/router';
import { FinancialAppComponent } from './financial.app.component';
import { IncomeComponent } from './pages/income/income.component';
import { AuthGuardService } from '../../../core/guards/auth.guard.ts.service';

export const FINANCIAL_ROUTES: Routes = [
  {
    path: '',
    component: FinancialAppComponent,
    children: [
        {
            path: 'income',
            component: IncomeComponent,
            canActivate: [AuthGuardService]
        }
    ],
  },

];
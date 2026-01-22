import { Component } from '@angular/core';

@Component({
  selector: 'app-income',
  standalone: true,
  imports: [],
  templateUrl: './income.component.html',
  styleUrls: ['./income.component.scss', '../../../../../../app.component.scss'],
})
export class IncomeComponent {
  title = 'Receitas';
  description = 'Gestão de Receitas, Pagamentos e Transferências Recebidas.';
}

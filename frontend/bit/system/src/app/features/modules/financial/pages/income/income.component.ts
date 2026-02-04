import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NzTableModule } from 'ng-zorro-antd/table';
import { FormatPipe } from '../../../../../core/pipes/format.pipe';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { endOfMonth } from 'date-fns';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NewFinancialMovComponent } from "../../components/new-financial-mov/new-financial-mov.component";

interface IncomeItem {
  date: string;
  dueDate: string;
  description: string;
  source: string;
  category: string;
  type: 'Credit' | 'Transfer';
  amount: number;
  paid: number;
  pending: number;
}

@Component({
  selector: 'app-income',
  standalone: true,
  imports: [
    CommonModule,
    NzTableModule,
    FormatPipe,
    NzDatePickerModule,
    NzSelectModule,
    NzInputModule,
    NzIconModule,
    NewFinancialMovComponent
],
  templateUrl: './income.component.html',
  styleUrls: [
    './income.component.scss',
    '../../../../../app.component.scss',
  ],
})
export class IncomeComponent {
  title = 'Receitas';
  description = 'Receitas, Pagamentos e Transferências.';

  dashboardCards = [
    {
      title: 'Total',
      description: 'Total de Receitas',
      route: '',
      value: 98500,
      icon: 'fa-solid fa-sack-dollar text-success',
    },
    {
      title: 'Pendente',
      description: 'Receitas Pendentes',
      route: '',
      value: 12400,
      icon: 'fa-solid fa-hand-holding-dollar text-warning',
    },
  ];

  listOfColumn = [
    {
      title: 'Lançamento',
      compare: (a: IncomeItem, b: IncomeItem) => a.date.localeCompare(b.date),
      priority: false,
    },
    {
      title: 'Vencimento',
      compare: (a: IncomeItem, b: IncomeItem) =>
        a.dueDate.localeCompare(b.dueDate),
      priority: false,
    },
    {
      title: 'Descrição',
      compare: (a: IncomeItem, b: IncomeItem) =>
        a.description.localeCompare(b.description),
      priority: false,
    },
    {
      title: 'Origem',
      compare: (a: IncomeItem, b: IncomeItem) =>
        a.source.localeCompare(b.source),
      priority: false,
    },
    {
      title: 'Categoria',
      compare: (a: IncomeItem, b: IncomeItem) =>
        a.category.localeCompare(b.category),
      priority: false,
    },
    {
      title: 'Tipo',
      compare: (a: IncomeItem, b: IncomeItem) => a.type.localeCompare(b.type),
      priority: false,
    },
    {
      title: 'Valor',
      compare: (a: IncomeItem, b: IncomeItem) => a.amount - b.amount,
      priority: 3,
    },
    {
      title: 'Pago',
      compare: (a: IncomeItem, b: IncomeItem) => a.paid - b.paid,
      priority: 2,
    },
    {
      title: 'Pendente',
      compare: (a: IncomeItem, b: IncomeItem) => a.pending - b.pending,
      priority: 1,
    },
  ];

  listOfData: IncomeItem[] = [
    {
      date: '2024-01-01',
      dueDate: '2024-01-05',
      description: 'January Salary',
      source: 'Company XYZ',
      category: 'Salary',
      type: 'Credit',
      amount: 5500,
      paid: 5500,
      pending: 0,
    },
    {
      date: '2024-01-03',
      dueDate: '2024-01-10',
      description: 'Freelance Project',
      source: 'Client Alpha',
      category: 'Freelance',
      type: 'Credit',
      amount: 3200,
      paid: 1600,
      pending: 1600,
    },
    {
      date: '2024-01-05',
      dueDate: '2024-01-15',
      description: 'Bank Transfer',
      source: 'Savings Account',
      category: 'Transfer',
      type: 'Transfer',
      amount: 2000,
      paid: 2000,
      pending: 0,
    },
    {
      date: '2024-01-06',
      dueDate: '2024-01-20',
      description: 'Online Course Sales',
      source: 'Platform EDU',
      category: 'Digital Products',
      type: 'Credit',
      amount: 1800,
      paid: 1200,
      pending: 600,
    },
    {
      date: '2024-01-07',
      dueDate: '2024-01-25',
      description: 'Affiliate Commission',
      source: 'Marketing Partner',
      category: 'Commission',
      type: 'Credit',
      amount: 950,
      paid: 950,
      pending: 0,
    },
  ];

  ranges = {
    Today: [new Date(), new Date()],
    'This Month': [new Date(), endOfMonth(new Date())],
  };

  isMobile = false;
  newFinancialMoveVisible = true;

  selectedValue = null;

  ngOnInit() {
    this.checkViewport();
    window.addEventListener('resize', () => this.checkViewport());
  }

  checkViewport() {
    this.isMobile = window.innerWidth < 992; // lg breakpoint
  }

  onChange(result: Date[]): void {
    console.log('From: ', result[0], ', to: ', result[1]);
  }

  trackColumn(_: number, column: any) {
    return column.title;
  }

  trackData(_: number, data: IncomeItem) {
    return data.date + data.description;
  }
}

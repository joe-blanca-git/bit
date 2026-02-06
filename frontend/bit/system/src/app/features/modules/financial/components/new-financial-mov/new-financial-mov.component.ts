import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { FormatPipe } from '../../../../../core/pipes/format.pipe';

interface Installment {
  number: number;
  dueDate: Date;
  value: number;
}

@Component({
  selector: 'app-new-financial-mov',
  standalone: true,
  imports: [
    CommonModule,
    NzButtonModule,
    NzModalModule,
    NzFormModule,
    NzSelectModule,
    NzInputNumberModule,
    NzDatePickerModule,
    NzTableModule,
    NzSwitchModule,
    ReactiveFormsModule,
    FormatPipe
  ],
  providers: [FormatPipe],
  templateUrl: './new-financial-mov.component.html',
  styleUrl: './new-financial-mov.component.scss',
})
export class NewFinancialMovComponent implements OnInit {
  @Input() isVisible: boolean = false;
  @Input() movType: number = 0;
  @Input() title: string = 'Nova Movimentação';
  @Output() close = new EventEmitter<void>();

  public financialForm!: FormGroup;
  public installments: Installment[] = [];

  listPerson: string[] = [];

  isLoadingData = false;

  readonly nzFilterOption = (): boolean => true;

  constructor(private fb: FormBuilder, private formatPipe: FormatPipe) {}

  ngOnInit(): void {
    this.initForm();
    this.watchChanges();
  }

  search(value: string): void {
    
  }

  async loadData(){
    this.isLoadingData = true;

    try {
      
    } catch (error) {
      
    } finally {
      this.isLoadingData = false;
    }
  }

  async getPerson(){

  }

  private initForm(): void {
    this.financialForm = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(3)]],
      type: [0, [Validators.required]],
      totalAmount: [0, [Validators.required, Validators.min(0.01)]],
      installmentsCount: [1, [Validators.required, Validators.min(1)]],
      firstDueDate: [new Date(), [Validators.required]],
      categoryId: [null, [Validators.required]],
      originId: [null, [Validators.required]],
      personId: [null, [Validators.required]],
      isPaid: [false]
    });
  }

  private watchChanges(): void {
    this.financialForm.valueChanges.subscribe(() => {
      this.generateInstallments();
    });
  }

  private generateInstallments(): void {
    const { totalAmount, installmentsCount, firstDueDate } = this.financialForm.value;

    if (!totalAmount || !installmentsCount || !firstDueDate) {
      this.installments = [];
      return;
    }

    const baseValue = Math.floor((totalAmount / installmentsCount) * 100) / 100;
    const remainder = parseFloat((totalAmount - baseValue * installmentsCount).toFixed(2));
    const list: Installment[] = [];

    for (let i = 0; i < installmentsCount; i++) {
      const date = new Date(firstDueDate);
      date.setMonth(date.getMonth() + i);

      list.push({
        number: i + 1,
        dueDate: date,
        value: i === 0 ? parseFloat((baseValue + remainder).toFixed(2)) : baseValue,
      });
    }
    this.installments = list;
  }

  handleOk(): void {
    if (this.financialForm.valid) {
      const payload = this.financialForm.value;

      if (payload.isPaid) {
        console.log('Executando 2 chamadas de API: Cadastro + Liquidação');
      } else {
        console.log('Executando 1 chamada de API: Apenas Cadastro');
      }

      this.close.emit();
    } else {
      Object.values(this.financialForm.controls).forEach((control) => {
        if (control.invalid) {
          control.markAsDirty();
          control.updateValueAndValidity({ onlySelf: true });
        }
      });
    }
  }

  handleCancel(): void {
    this.close.emit();
  }

  formatterReal = (value: number | string): string => {
    return this.formatPipe.transform(value, 'currency');
  };

  parserReal = (value: string): string => {
    return value
      .replace('R$', '')
      .replace(/\./g, '')
      .replace(',', '.')
      .trim();
  };

  trackByItem(index: number, item: any) {
  return item;
}

}
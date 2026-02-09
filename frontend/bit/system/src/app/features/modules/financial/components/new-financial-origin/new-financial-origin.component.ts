import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinancialService } from '../../services/financial.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';

@Component({
  selector: 'app-new-financial-origin',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NzButtonModule, NzModalModule, NzFormModule],
  templateUrl: './new-financial-origin.component.html',
  styleUrl: './new-financial-origin.component.scss',
})
export class NewFinancialOriginComponent {
  @Input() isVisible: boolean = false;
  @Input() title: string = 'Nova Origem Financeira';
  @Output() close = new EventEmitter<void>();

  public newFinancialOrigin!: FormGroup;
  isLoadingData = false;

  constructor(
    private fb: FormBuilder,
    private financialService: FinancialService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.newFinancialOrigin = this.fb.group({
      description: ['', [Validators.required]],
    });
  }

    handleOk(): void {
    // if (this.financialForm.valid) {
    //   const payload = this.financialForm.value;

    //   if (payload.isPaid) {
    //     console.log('Executando 2 chamadas de API: Cadastro + Liquidação');
    //   } else {
    //     console.log('Executando 1 chamada de API: Apenas Cadastro');
    //   }

    //   this.close.emit();
    // } else {
    //   Object.values(this.financialForm.controls).forEach((control) => {
    //     if (control.invalid) {
    //       control.markAsDirty();
    //       control.updateValueAndValidity({ onlySelf: true });
    //     }
    //   });
    // }
  }

    handleCancel(): void {
    this.close.emit();
  }
}

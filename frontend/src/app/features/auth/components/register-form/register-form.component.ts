import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  AbstractControl,
  AsyncValidatorFn,
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { NgxMaskDirective, NgxMaskPipe, provideNgxMask } from 'ngx-mask';
import { map, catchError, debounceTime, switchMap, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-register-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgxMaskDirective,
    NgxMaskPipe,
  ],
  templateUrl: './register-form.component.html',
  styleUrl: './register-form.component.scss',
  providers: [provideNgxMask()],
})
export class RegisterFormComponent {
  formRegister: FormGroup;
  submitted = false;
  selectedUserType: string = '';

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.formRegister = this.fb.group({
      name: ['', [Validators.required]],
      document: [
        '',
        [Validators.required, Validators.maxLength(14)],
        [this.validateExistingDocument(this.authService)],
      ],
      email: [
        '',
        [Validators.required, Validators.email],
        [this.validateExistingEmail(this.authService)],
      ],
      phone: ['', [Validators.required]],
      userType: ['self', Validators.required],
      nameCompany: [''],
    });

    this.formRegister.get('userType')?.valueChanges.subscribe((value) => {
      this.selectedUserType = value;

      if (value === 'self') {
        this.formRegister.get('nameCompany')?.reset();
      }
    });
  }

  get f() {
    return this.formRegister.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.formRegister.markAllAsTouched();

    if (this.formRegister.invalid) {
      return;
    }

    console.log('Conta válida', this.formRegister.value);
  }

  validateExistingDocument(authService: AuthService): AsyncValidatorFn {
    return (control: AbstractControl) => {
      const cpf = control.value?.replace(/\D/g, '');
      
      if (!cpf || !this.validateDocumentLocal(cpf)) {
        return of({ documentInvalid: true });
      }

      return of(cpf).pipe(
        debounceTime(400),
        switchMap(() => authService.verifyExitingDocument(cpf, 'c')),
        map((res) => (res.exists ? { documentExisting: true } : null)),
        catchError(() => of(null))
      );
    };
  }

  validateExistingEmail(authService: AuthService): AsyncValidatorFn {
    return (control: AbstractControl) => {
      const email = control.value;
      if (!email) return of(null);

      return of(email).pipe(
        debounceTime(400),
        switchMap(() => authService.verifyExitingDocument(email, 'e')),
        map((res) => (res.exists ? { emailExisting: true } : null)),
        catchError(() => of(null))
      );
    };
  }

  private validateDocumentLocal(cpf: string): boolean {
    cpf = cpf.replace(/[^\d]+/g, '');
    if (cpf.length !== 11 || /^(\d)\1+$/.test(cpf)) return false;

    let soma = 0;
    for (let i = 0; i < 9; i++) soma += parseInt(cpf.charAt(i)) * (10 - i);
    let resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.charAt(9))) return false;

    soma = 0;
    for (let i = 0; i < 10; i++) soma += parseInt(cpf.charAt(i)) * (11 - i);
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    return resto === parseInt(cpf.charAt(10));
  }
}

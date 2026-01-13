import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core'; // Importar OnInit
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

// Definição da classe (como você mencionou) para referência no payload
export class RegisterUser {
  fullname!: string;
  email!: string;
  username!: string;
  password!: string;
  cpf!: string;
  dtBirth!: string;
  address!: string;
  cep!: string;
  idCity!: number;
  idState!: number;
  company_name!: string;
}

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
export class RegisterFormComponent implements OnInit { 
  formRegister: FormGroup;
  submitted = false;
  selectedUserType: string = 'self';

  states: any[] = [];
  cities: any[] = [];
  allCities: any[] = [
    { id: 1, stateId: 1, name: 'São Paulo' },
    { id: 2, stateId: 1, name: 'Campinas' },
    { id: 3, stateId: 2, name: 'Rio de Janeiro' },
    { id: 4, stateId: 2, name: 'Niterói' },
    { id: 5, stateId: 3, name: 'Belo Horizonte' },
  ];

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.formRegister = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email], [this.validateExistingEmail(this.authService)],],
      document: ['', [Validators.required, Validators.maxLength(14)], [this.validateExistingDocument(this.authService)],],
      phone: ['', [Validators.required]],
      userType: ['self', Validators.required],
      nameCompany: [''],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        this.passwordStrengthValidator
      ]],
      passwordConfirmation: ['', [Validators.required]],
      dtBirth: ['', [Validators.required]],
      cep: ['', [Validators.required]],
      address: ['', [Validators.required]],
      idState: ['', [Validators.required]],
      idCity: ['', [Validators.required]],
    }, {
      validators: this.passwordMatchValidator
    });

    this.formRegister.get('userType')?.valueChanges.subscribe((value) => {
      this.selectedUserType = value;
      const nameCompanyControl = this.formRegister.get('nameCompany');

      if (value === 'company') {
        nameCompanyControl?.setValidators([Validators.required]);
      } else {
        nameCompanyControl?.clearValidators();
        nameCompanyControl?.reset();
      }
      nameCompanyControl?.updateValueAndValidity();
    });

    this.formRegister.get('idState')?.valueChanges.subscribe((stateId) => {
      this.formRegister.get('idCity')?.reset('');
      if (stateId) {
        this.cities = this.allCities.filter(c => c.stateId == stateId);
      } else {
        this.cities = [];
      }
    });
  }

  ngOnInit(): void {
    this.states = [
      { id: 1, name: 'SP' },
      { id: 2, name: 'RJ' },
      { id: 3, name: 'MG' },
    ];
  }

  get f() {
    return this.formRegister.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.formRegister.markAllAsTouched();

    if (this.formRegister.invalid) {
      console.log('Formulário inválido', this.formRegister.errors);
      return;
    }

    const formValue = this.formRegister.value;
    const payload: RegisterUser = {
      fullname: formValue.name,
      email: formValue.email,
      username: formValue.email,
      password: formValue.password,
      cpf: formValue.document.replace(/\D/g, ''), 
      dtBirth: this.reformatDate(formValue.dtBirth),
      address: formValue.address,
      cep: formValue.cep.replace(/\D/g, ''),
      idCity: +formValue.idCity,
      idState: +formValue.idState,
      company_name: formValue.userType === 'self'
        ? formValue.name
        : formValue.nameCompany
    };

    console.log('Conta válida. Payload para API:', JSON.stringify(payload));

    // this.authService.register(payload).subscribe(...)
  }

  // --- Validadores Assíncronos (sem mudança) ---
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

  // --- Validador Local de CPF (sem mudança) ---
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

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value: string = control.value;
    if (!value) {
      return null;
    }

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);

    const isValid = hasUpperCase && hasLowerCase && hasNumeric;

    if (!isValid) {
      // Retorna os erros específicos para feedback no HTML
      return {
        passwordStrength: {
          hasUpperCase,
          hasLowerCase,
          hasNumeric
        }
      };
    }

    return null;
  }

  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const passwordConfirmation = group.get('passwordConfirmation')?.value;

    if (password !== passwordConfirmation) {
      group.get('passwordConfirmation')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
       if (group.get('passwordConfirmation')?.hasError('passwordMismatch')) {
         group.get('passwordConfirmation')?.setErrors(null);
       }
      return null;
    }
  }

  private reformatDate(dateStr: string): string {
    if (!dateStr || dateStr.length !== 8) {
      return dateStr || '';
    }
    
    const day = dateStr.substring(0, 2);
    const month = dateStr.substring(2, 4);
    const year = dateStr.substring(4, 8);

    return `${year}-${month}-${day}`;
  }
}
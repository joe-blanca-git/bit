import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.scss']
})
export class LoginFormComponent {
  formLogin: FormGroup;
  submitted = false;

  constructor(private fb: FormBuilder) {
    this.formLogin = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  get f() {
    return this.formLogin.controls;
  }

  onSubmit() {
    this.submitted = true;

    this.formLogin.markAllAsTouched();

    if (this.formLogin.invalid) {
      return;
    }

    console.log('Login válido:', this.formLogin.value);
  }
}

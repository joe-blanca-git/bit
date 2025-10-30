import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RegisterFormComponent } from "../../components/register-form/register-form.component";

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, RegisterFormComponent],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss', '../../auth.app.component.scss']
})
export class RegisterComponent {

}

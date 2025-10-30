import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {LoginFormComponent} from '../../components/login-form/login-form.component'
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, LoginFormComponent, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss', '../../auth.app.component.scss']
})
export class LoginComponent {

}

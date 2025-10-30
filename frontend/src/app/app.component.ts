import { CommonModule } from '@angular/common';
import { Component, HostBinding } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  // @HostBinding('class') get getClass() {
  //   return Object.keys(this.screen.sizes)
  //     .filter((cl) => this.screen.sizes[cl])
  //     .join(' ');
  // }
  
  constructor() //private authService: AuthService,
  //private screen: ScreenService
  {

  }

  ngOnInit(): void {

  }

  isAuthenticated() {
    //return this.authService.loggedIn;
  }
}

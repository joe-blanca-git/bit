import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";

@Component({
    selector: 'home-app-root',
    templateUrl: './home.app.component.html',
    styleUrls: ['./home.app.component.scss'],
    standalone: true,
    imports: [
        CommonModule,
    ]
    
})
export class HomeAppComponent {
    
}
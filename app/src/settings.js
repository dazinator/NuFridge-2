export class Settings{
  constructor(){
    this.heading = 'Settings';
    this.firstName = 'John';
    this.lastName = 'Doe';
  }

  get fullName(){
    return `${this.firstName} ${this.lastName}`;
  }

  welcome(){
    alert(`Welcome, ${this.fullName}!`);
  }
}

export class UpperValueConverter {
  toView(value){
    return value && value.toUpperCase();
  }
}

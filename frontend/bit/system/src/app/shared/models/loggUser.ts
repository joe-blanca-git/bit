
export class loggUser {
  email?: string;
  name?: string;
  document?: string;
  claims?: claims[];
}

export class claims {
  value?: string;
  type?: string;
}

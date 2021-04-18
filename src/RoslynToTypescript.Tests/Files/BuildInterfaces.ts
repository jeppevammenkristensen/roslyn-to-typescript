export interface TestClass {
   info? : OtherClass;
   items : string[];
   otherClass : OtherClass[];
}

export interface OtherClass {
   name : string;
}
namespace Zios.Supports.Mixed{
	public class Mixed{
		public static Mixed<A> Create<A>(A a){return new Mixed<A>(a);}
		public static Mixed<A,B> Create<A,B>(A a,B b){return new Mixed<A,B>(a,b);}
		public static Mixed<A,B,C> Create<A,B,C>(A a,B b,C c){return new Mixed<A,B,C>(a,b,c);}
		public static Mixed<A,B,C,D> Create<A,B,C,D>(A a,B b,C c,D d){return new Mixed<A,B,C,D>(a,b,c,d);}
	}
	public class Mixed<A>{
		public A a;
		public Mixed(){}
		public Mixed(A a){
			this.a = a;
		}
	}
	public class Mixed<A,B> : Mixed<A>{
		public B b;
		public Mixed(){}
		public Mixed(A a,B b) : base(a){
			this.b = b;
		}
	}
	public class Mixed<A,B,C> : Mixed<A,B>{
		public C c;
		public Mixed(){}
		public Mixed(A a,B b,C c) : base(a,b){
			this.c = c;
		}
	}
	public class Mixed<A,B,C,D> : Mixed<A,B,C>{
		public D d;
		public Mixed(){}
		public Mixed(A a,B b,C c,D d) : base(a,b,c){
			this.d = d;
		}
	}
}
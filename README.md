# DeepCloner

Library with extenstion to clone objects for .NET. It can deep or shallow copy objects. In deep cloning all object graph is maintained. Library actively uses code-generation in runtime as result object cloning is blazingly fast.
Also, there are some performance tricks to increase cloning speed (see tests below).
Objects are copied by its' internal structure, **no** methods or constructuctors are called for cloning objects. As result, you can copy **any** object, but we don't recommend to copy objects which are binded to native resources or pointers. It can cause unpredictable results (but object will be cloned).

You don't need to mark objects somehow, like Serializable-attribute, or restrict to specific interface. Absolutely any object can be cloned by this library. And this object doesn't have any ability to determine that he is clone (except with very specific methods).

Also, there is no requirement to specify object type for cloning. Object can be casted to inteface or as an abstract object, you can clone array of ints as abstract Array or IEnumerable, even null can be cloned without any errors.

Installation through Nuget:

```
	Install-Package DeepCloner
```


## Supported Frameworks

DeepCloner works for .NET 4.0 or higher or for .NET Standard 1.3 (.NET Core). .NET Standard version implements only Safe copying variant (slightly slower than standard, see Benchmarks).

## Limitation

Library requires Full Trust permission set or Reflection permission (MemberAccess). It prefers Full Trust, but if code lacks of this variant, library seamlessly switchs to slighlty slower but safer variant.

If your code is on very limited permission set, you can try to use another library, e.g. [CloneExtensions](https://github.com/MarcinJuraszek/CloneExtensions). It clones only public properties of objects, so, result can differ, but should work better (it requires only RestrictedMemberAccess permission).

## Usage

Deep cloning any object: 
```
  var clone = new { Id = 1, Name = "222" }.DeepClone();
```

With a reference to same object:
```
  // public class Tree { public Tree ParentTree;  }
  var t = new Tree();
  t.ParentTree = t;
  var cloned = t.DeepClone();
  Console.WriteLine(cloned.ParentTree == cloned); // True
```

Or as object:
```
  var date = DateTime.Now;
  var obj = (object)date;
  obj.DeepClone().GetType(); // DateTime
```

Shallow cloning (clone only same object, not objects that object relate to) 
```
  var clone = new { Id = 1, Name = "222" }.ShallowClone();
```

Cloning to existing object (can be useful for _copying_ constructors, creating wrappers or for keeping references to same object)
```
public class Derived : BaseClass
{
	public Derived(BaseClass parent)
	{
		parent.DeepCloneTo(this); // now this has every field from parent
	}
}
```
Please, note, that _DeepCloneTo_ and _ShallowCloneTo_ requre that object should be class (it is useless for structures) and derived class must be real descendant of parent class (or same type). In another words, this code will not work:
```
public class Base {}
public class Derived1 : Base {}
public class Derived2 : Base {}

var b = (Base)new Derived1(); // casting derived to parent
var derived2 = new Derived2();
// will compile, but will throw an exception in runtime, Derived1 is not parent for Derived2
b.DeepCloneTo(derived2); 

```  

## Installation

Through nuget: 
```
  Install-Package DeepCloner
```

## Details

You can use deep clone of objects for a lot of situations, e.g.:
* Emulation of external service or _deserialization elimination_ (e.g. in Unit Testing). When code has received object from external source, code can change it (because object for code is *own*).
* ReadOnly object replace. Instead of wrapping your object to readonly object, you can clone object and target code can do anything with it without any restriction.
* Caching. You can cache data locally and want to ensurce that cached object hadn't been changed by other code
 
You can use shallow clone as fast, light version of deep clone (if your situation allows that). Main difference between deep and shallow clone in code below:
```
  // public class A { public B B; }
  // public class B { public int X; }
  var b = new B { X = 1 };
  var a = new A { B = b };
  var deepClone = a.DeepClone();
  deepClone.B.X = 2;
  Console.WriteLine(a.B.X); // 1
  var shallowClone = a.ShallowClone();
  shallowClone.B.X = 2;
  Console.WriteLine(a.B.X); // 2
```
So, deep cloning is guarantee that all changes of cloned object does not affect original. Shallow clone does not guarantee this. But it faster, because deep clone of object can copy big graph of related objects and related objects of related objects and related related related objects, and... so on...

This library does not call any method of cloning object: constructors, Equals, GetHashCode, propertes - nothing is called. So, it is impossible for cloning object to receive information about cloning, throw an exception or return invalid data. 
If you need to call some methods after cloning, you can wrap cloning call to another method which will perform required actions.

Extension methods in library are generic, but it is not require to specifify type for cloning. You can cast your objects to System.Object, or to an interface, add fields will be carefully copied to new object.

### Performance 
Cloning Speed can vary on many factors. This library contains some optimizations, e.g. structs are just copied, arrays also can be copied through Array.Copy if possible. So, real performance will depend on structure of your object.

Tables below, just for information. Simple object with some fields is cloned multiple times. Preparation time (only affect first execution) excluded from tests.

Example of object
```
var c = new C1 { V1 = 1, O = new object(), V2 = "xxx" };
var c1 = new C1Complex { C1 = c, Guid = Guid.NewGuid(), O = new object(), V1 = 42, V2 = "some test string", Array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } };
```


**Deep cloning** 

  Method   |  Time per object (ns)  | Comments
---|---|---
Manual | 50 |  You should manually realize cloning. It requires a lot of work and can cause copy-paste errors, but it is fastest variant
DeepClone / Unsafe | 570 | This variant is really slower than manual, but clones any object without preparation
DeepClone / Safe | 760 | Safe variant based on on expressions
[CloneExtensions](https://github.com/MarcinJuraszek/CloneExtensions) | 1800 | Implementation of cloning objects on expression trees.
[NClone](https://github.com/mijay/NClone) | 2890 | Not analyzed carefully, but author says that lib has a problem with a cyclic dependencies
[Clone.Behave!](https://github.com/kalisohn/CloneBehave) | 41890 | Very slow, also has a dependency to fasterflect
[GeorgeCloney](https://github.com/laazyj/GeorgeCloney) | 6420 | Has a lot limitations and prefers to clone through BinaryFormatter
[Nuclex.Cloning](https://github.com/junweilee/Nuclex.Cloning/) | n/a | Crashed with a null reference exception
[.Net Object FastDeepCloner](https://github.com/Alenah091/FastDeepCloner/) | 15030 | Not analyzed carefully, only for .NET 4.5.1 or higher
[DesertOctopus](https://github.com/nowol/DesertOctopus) | 1700 | Not analyzed. Only for .NET 4.5.2 or higher
BinaryFormatter | 49100 | Another way of deep object cloning through serializing/deserializing object. Instead of Json serializers - it maintains full graph of serializing objects and also do not call any method for cloning object. But due serious overhead, this variant is very slow

**Shallow cloning** 
Shallow cloning is usually faster, because we no need to calculate references and clone additional objects.

  Method   |  Time per object (ns)  | Comments
---|---|---
Manual | 16 | You should manually realize clone, property by property, field by field. Fastest variant
Manual / MemberwiseClone | 46 | Fast variant to clone: call MemberwiseClone inside your class. Should be done manually, but does not require a lot of work.
ShallowClone / Unsafe | 64 | Slightly slower than MemberwiseClone due checks for nulls and object types
ShallowClone / Safe | 64 | Safe variant based on expressions
[CloneExtensions](https://github.com/MarcinJuraszek/CloneExtensions) | 125 | Implementation of cloning objects on expression trees.
[Nuclex.Cloning](https://github.com/junweilee/Nuclex.Cloning/) | 2498 | Looks like interesting expression-based implementation with a some caching, but unexpectedly very slow

## Performance tricks

We perform a lot of performance tricks to ensure cloning is really fast. Here is some of them:

* Using a shallow cloning instead of deep cloning if object is safe for this operation
* Copying an whole object and updating only required fields
* Special handling for structs (can be copied without any cloning code, if possible)
* Cloners caching
* Optimizations for copying simple objects (reduced number of checks to ensure good performance)
* Special handling of reference count for simple objects, that is faster than default dictionary
* Constructors analyzing to select best variant of object construction
* Direct copying of arrays if possible
* Custom handling of one-dimensional and two-dimensional zero-based arrays (most of arrays in usual code)


## License

[MIT](https://github.com/force-net/DeepCloner/blob/develop/LICENSE) license

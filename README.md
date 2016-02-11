# DeepCloner
===============

Library with extenstion to clone objects for .NET. It can deep or shallow copy object. In deep cloning all object graph is maintained. Library using code-generation in runtime as result object cloning is really fast.
Objects are copied by its' internal structure, **no** methods or constructuctors are called for cloning objects. As result, you can copy **any** object, but we don't recommend to copy objects which are binded to native resources or pointers. It can cause unpredictable results (but object will be cloned).

You don't need to mark objects somehow, like Serializable-attribute, or restrict to specific interface. Absolutely any object can be cloned by this library. And this object doesn't have any ability to determine that he is clone (except very specific methods).

## Limitation

Library requires Full Trust permission set or Reflection permission (RestrictedMemberAccess + MemberAccess). It prefers Full Trust, but if code lacks of this variant, library seamlessly switchs to slighlty slower but safer variant.

If your code is on very limited permission set, you can try to use another library, e.g. [CloneExtensions](https://github.com/MarcinJuraszek/CloneExtensions). It clones only public properties of objects, so, result can differ, but it works anywhere.



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
  var object = (object)date;
  object.DeepClone().GetType(); // DateTime
```

Shallow cloning (clone only same object, not objects that object relate to) 
```
  var clone = new { Id = 1, Name = "222" }.ShallowClone();
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

Tables below, just for information. Simple object with some fields ara cloned multiple times. Preparation time (only affect first execution) excluded from tests.

**Deep cloning** 

  Method   |  Time (in ms)  | Comments
---|---|---
Manual | 13 |  You should manually realize cloning. It requires a lot of work and can cause copy-paste errors, but it is fastest variant
DeepClone / Unsafe | 331 | This variant is really slower than manual, but clones any object without preparation
DeepClone / Safe | 411 | Safe variant based on on expressions
[CloneExtensions](https://github.com/MarcinJuraszek/CloneExtensions) | 560 | Implementation of cloning objects on expression trees.
BinaryFormatter | 15000 | Another way of deep object cloning through serializing/deserializing object. Instead of Json serializers - it maintains full graph of serializing objects and also do not call any method for cloning object. But due serious overhead, this variant is very slow

**Shallow cloning** 
Shallow cloning is usually faster, because we no need to calculate references and clone additional objects.

  Method   |  Time (in ms)  | Comments
---|---|---
Manual | 11 | You should manually realize clone, property by property, field by field. Fastest variant
Manual / MemberwiseClone | 37 | Fast variant to clone: call MemberwiseClone inside your class. Should be done manually, but does not require a lot of work.
ShallowClone / Unsafe | 46 | Slightly slower than MemberwiseClone due checks for nulls and object types
ShallowClone / Safe | 48 | Safe variant based on expressions
[CloneExtensions](https://github.com/MarcinJuraszek/CloneExtensions) | 123 | Implementation of cloning objects on expression trees.

## License

[MIT](https://github.com/force-net/DeepCloner/blob/develop/LICENSE) license

//jquery插件开发的两种方法：
//一种是类级别的插件开发，即给jQuery添加新的全局函数，相当于给jQuery类本身添加方法。jQuery的全局函数就是属于jQuery命名空间的函数，另一种是对象级别的插件开发，即给jQuery对象添加方法。下面就两种函数的开发做详细的说明。
//$.fn.extend是为查询的节点对象扩展方法，是基于$的原型扩展的方法

//$.extend是扩展常规方法，是$的静态方法。
//一、类级别的插件开发：
//类级别的插件开发最直接的理解就是给jQuery类添加类方法，可以理解为添加静态方法。
//典型的例子就是$.ajax()、$.getJson()，将函数定义于jQuery的命名空间中。
//关于类级别的插件开发可以采用如下几种形式进行扩展：
//1.1 添加一个新的全局函数
jQuery.foo = function () {
    alert('This is a test. This is only a test.');
};//调用：jQuery.foo();

//1.2 增加多个全局函数
jQuery.foo = function () {
    alert('This is a test. This is only a test.');
};
jQuery.bar = function (param) {
    alert('This function takes a parameter, which is "' + param + '".');
};//调用时和一个函数的一样的: jQuery.foo(); jQuery.bar(); 或者$.foo(); $.bar('bar');

//1.3 使用jQuery.extend(object);<其它插件.js中也有这个jQuery.extend({foo:function(){}})怎么办>
jQuery.extend({
    foo: function () {
        alert('This is a test. This is only a test.');
    },
    bar: function (param) {
        alert('This function takes a parameter, which is "' + param + '".');
    }
});

//1.4 使用命名空间  避免冲突
//虽然在jQuery命名空间中，我们禁止使用了大量的javaScript函数名和变量名。
//但是仍然不可避免某些函数或变量名将于其他jQuery插件冲突，
//因此习惯将一些方法封装到另一个自定义的命名空间，这样可以避免重名冲突。
jQuery.myPlugin = {
    foo: function () {
        alert('This is a test. This is only a test.');
    },
    bar: function (param) {
        alert('This function takes a parameter, which is "' + param + '".');
    }
};
//采用命名空间的函数仍然是全局函数，调用时采用的方法：
//通过这个技巧（使用独立的插件名），可以避免命名空间内函数的冲突。
$.myPlugin.foo();
$.myPlugin.bar('baz');
///////////////////////////////////////////////////////////////////////////////////////////////
//二、对象级别的插件开发：<用法：$('#btn').myPlugname();>
//1.定义一个无参的对象级jquery扩展方法：
//$.fn.extend 就相当于$.prototype原型
(function ($) {
    $.fn.extend({
        myPlugname: function () {
            $(this).click(function () {
                alert($(this).val());
            });
            //dosomething;
        },

        myPlugname2: function () {
            //dosomething;
        }

        //其它功能函数
    });
})(jQuery);   //匿名的自执行函数:(function($){})(jQuery)：形参$,jQuery实参，实参传入后,插件内就可以使用$符号了;
//上式等价于：
(function ($) {
    $.fn.myPlugname3 = function () {
        $(this).click(function () {
            alert($(this).val());
        });
        //dosomething;
    };

    $.fn.myPlugname4 = function () {
        //dosomething;
    };
})(jQuery);
//2. 定义一个有参的对象级jquery扩展方法：
//2. 调用：$('#mydiv').hilight({foreground:blue''});
(function ($) {
    $.fn.hilight = function (options) {
        //自带的默认参数
        var defaults = {
            foreground: 'red',
            background: 'yellow'
        };
        //把defaults的属性和options的属性合并并保存到opts中。
        var opts = $.extend(defaults, options);
        //把用户传的参数options追加到defaults上，(如果options中也有foreground属性,则覆盖掉defaults中的参数)；其实就相当于是C#中：public void add(int a = 1,int b=3){}；调用时：add(a=3)一样

        //dosomething
        $(this).css("background-color", opts.background);
        $(this).css("color", opts.foreground);
    };
})(jQuery); 
//*******************(function($){...})(jQuery)是什么意思 *******************//
//这里实际上是匿名函数
//function(arg){...}
//这就定义了一个匿名函数，参数为arg
//而调用函数 时，是在函数后面写上括号和实参的，由于操作符的优先级，函数本身也需要用括号，即：
//(function(arg){...})(param)
//这 就相当于定义了一个参数为arg的匿名函数，并且将param作为参数来调用这个匿名函数

//而(function($){...}) (jQuery)则是一样的，之所以只在形参使用$，是为了不与其他库冲突，所以实参用jQuery

//**************************************************************************
//其实就等于
//var fn = function($){....};
//fn(jQuery);
//其实可以这么理解，不过要注意的是fn是不存在的
//那个函数直接定义，然后就运行了。就“压缩”成下面的样子了
//(function($){...})(jQuery) 
//**************************************************************************
//简单理解是(function($){...})(jQuery)用来定义一些需要预先定义好的函数
//$(function(){ })则是用来在DOM加载完成之后运行\执行那些预行定义好的函数.
//**************************************************************************
//开发jQuery插件时总结的一些经验分享一下。 

//一、先看 
//jQuery(function(){ 
//}); 
//全写为 
//jQuery(document).ready(function(){ 

//}); 
//意义为在DOM加载完毕后执行了ready()方法。 

//二、再看 
//(function(){ 
//})(jQuery)； 
//其实际上是执行()(para)匿名方法,只不过是传递了jQuery对象。 

//三、总结 
//jQuery(function(){　});用于存放操作DOM对象的代码，执行其中代码时DOM对象已存在。不可用于存放开发插件的代码，因为jQuery对象没有得到传递，外部通过jQuery.method也调用不了其中的方法（函数）。 
//(function(){　})(jQuery);用于存放开发的插件代码，执行其中代码时DOM不一定存在，所以直接自动执行DOM操作的代码请小心使用。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//在Jquery中，$是JQuery的别名，所有使用$的地方也都可以使用JQuery来替换，如$('#msg')等同于JQuery('#msg')的写法。然而，当我们引入多个js库后，在另外一个js库中也定义了$符号的话，那么我们在使用$符号时就发生了冲突。下面以引入两个库文件jquery.js和prototype.js为例来进行说明。

//第一种情况：jquery.js在prototype.js之后进行引入,如：
//<script src="prototype.js" type="text/javascript"/>
//<script src="jquery.js" type="text/javascript"/>

//在这种情况下,我们在自己的js代码中如下写的话：

//$('#msg').hide();

//$永远代表的是jquery中定义的$符号，也可以写成JQuery('#msg').hide();如果想要使用prototype.js中定义的$,我们在后面再介绍。


//第二种情况：jquery.js在prototype.js之前进行引入,如：
//<script src="jquery.js" type="text/javascript"/>
//<script src="prototype.js" type="text/javascript"/>

//在这种情况下,我们在自己的js代码中如下写的话：

//$('#msg').hide();

//$此时代表的prototype.js中定义的$符号，如果我们想要调用jquery.js中的工厂选择函数功能的话，只能用全称写法JQuery('#msg').hide().

//下面先介绍在第一种引入js库文件顺序的情况下，如何正确的使用不同的js库中定义的$符号。

//一.使用JQuery.noConflict()
//该方法的作用就是让Jquery放弃对$的所有权，将$的控制权交还给prototype.js,因为jquery.js是后引入的，所以最后拥有$控制权的是jquery。它的返回值是JQuery。当在代码中调用了该 方法以后，我们就不可以使用$来调用jquery的方法了，此时$就代表在prototype.js库中定义的$了。如下：

//JQuery.noConflict();

////此处不可以再写成$('#msg').hide(),此时的$代表prototype.js中定义的$符号。
//JQuey('#msg').hide();

//自此以后$就代表prototype.js中定义的$,jquery.js中的$无法再使用,只能使用jquery.js中$的全称JQuery了。


//二.自定义JQuery的别名
//如果觉得第一种方法中使用了JQuery.noConflict()方法以后,只能使用JQuery全称比较麻烦的话，我们还可以为JQuery重定义别名。如下：

//var $j=JQuery.noConflict();
//$j('#msg').hide();//此处$j就代表JQuery
//自此以后$就代表prototype.js中定义的$,jquey.js中的$无法再使用,只能使用$j来作为jquey.js中JQuery的别名了。


//三.使用语句块，在语句块中仍然使用jquery.js中定义的$，如下：
//JQuery.noConflict();
//JQuery(document).ready(function($){
//    $('#msg').hide();//此时在整个ready事件的方法中使用的$都是jquery.js中定义的$.
//});

//或者使用如下语句块：

//(function($){
//    .....
//    $('#msg').hide();//此时在这个语句块中使用的都是jquery.js中定义的$.
//})(JQuery)

//如果在第二种引入js库文件顺序的情况下,如何使用jquery.js中的$,我们还是可以使用上面介绍的语句块的方法，如：
//复制代码 代码如下:

//<script src="jquery.js" type="text/javascript"/>
//<script src="prototype.js" type="text/javascript"/>
//<script type="text/javascript">

//(function($){
//    .....
//    $('#msg').hide();//此时在这个语句块中使用的都是jquery.js中定义的$.
//})(JQuery)
//</script>

//这种使用语句块的方法非常有用，在我们自己写jquery插件时,应该都使用这种写法，因为我们不知道具体工作过程中是如何顺序引入各种js库的,而这种语句块的写法却能屏蔽冲突。

//(function($){})(jQuery)

//1 首先(function(){})()这种写法 是创建了一个匿名的方法并立即执行（function(){})这个是匿名方法后面的括号就是立即调用了这个方法）。
//这样做可以创建一个作用域以保证内部变量与外部变量不发生冲突，比如$ jQuery 等jquery内部定义的变量。

//2 (function($){})(jQuery) 这个写法主要的作用还是保证jquery不与其他类库或变量有冲突 首先是要保证jQuery这个变量名与外部没有冲突（jquery内部$与jQuery是同一个东西 有两个名字的原因就是怕$与其他变量名有冲突二jQuery与其他变量冲突的几率非常小）并传入匿名对象，匿名对象给参数起名叫做$(其实和jquery内部是一样的) 然后你就可以自由的在(function($){})(jQuery)里写你的插件而不需要考虑与外界变量是否存在冲突
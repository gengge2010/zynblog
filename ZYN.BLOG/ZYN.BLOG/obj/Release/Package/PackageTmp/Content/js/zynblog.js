//$(function(){})是页面一ready就执行
jQuery(function () {
    zyn.init();
});


var zyn = {
    //init是初始化函数；相当于C#的构造函数
    init: function () {
        this.autoComplete(),
        this.viewStatistic(),
        this.pariseShow();
        this.goTop();
        this.dropDown();
        this.panelToggle();
        this.panelClose();
        this.commentValidate();
        this.ajaxCommentsReply();
    },

    //搜索自动补全;给搜索框注册自动联想完成事件
    autoComplete: function () {
        jQuery('#searchWords').typeahead({
            source: function (query, process) {
                //query是输入值
                query = jQuery.trim(query);
                if (query == ' ' || query == null || query == undefined) {
                    ////////
                } else {
                    jQuery.getJSON('/Search/GetHotSearchItems', { "query": query }, function (data) {
                        process(data);
                    });
                }
            },
            updater: function (item) {
                return item.replace(/<a(.+?)<\/a>/, ""); //这里一定要return，否则选中不显示，外加调用display的时候null reference错误。
            },
            afterSelect: function (item) {
                //选择项之后的时间，item是当前选中的项
                alert(item);
            },
            items: 8,   //显示8条
            delay: 500  //延迟时间
        });
    },

    //阅读量统计 
    viewStatistic: function () {
        var jq = jQuery.noConflict();
        //点击文章详情链接<a class="gotoArchive">时,先发送post请求进行浏览量计数，再跳转
        jq(document).on('click', '.gotoArchive', function (e) {
            e.preventDefault();  //preventDefault()方法阻止元素发生默认的行为。e为必需，规定阻止哪个事件的默认动作。
            var aUrl = jq(this).attr('href');
            //alert(aUrl);
            var articleId = (aUrl.split("/"))[3];
            //alert(articleId);
            jq.post("/Archives/ViewStatic", { "artId": articleId }, function (data) {
                //alert('ok');
                window.location.href = aUrl; //无论后台计数成功与否，都得给用户跳转
            });
        });
    },

    //点赞特效+Ajax统计点赞数量
    pariseShow: function () {
        //使用自定义的点赞特效插件,在zynblog.js前要先引入这个插件
        //jquery给暂未生成的标签绑定事件要用on('事件','对象','事件句柄')
        jQuery(document).on("click", ".praisebtn", function (e) {
            e.preventDefault();
            //获取被点赞文章的id  praise-flag:0没攒过，1：赞过了
            //页面刚生成时，可以从库中确定该用户是否点赞，并为praise-flag属性赋初值
            //这里没必要那么严谨，所以初值均为1，(顶多是再在cookie中给个标记)
            var praiseFlag = jQuery(this).children('a').attr('praise-flag');
            //alert(praiseFlag);
            var praiseArtId = jQuery(this).children('a').attr('data-id');
            //alert(praiseArtId);

            //1. 如果没赞过
            if (praiseFlag == 0) {
                var curPraise = jQuery(this).children('a');
                curPraise.attr('praise-flag', "1");//先把点赞标识的属性值设为1

                jQuery(this).praise({
                    obj: jQuery(this),
                    str: "+1",
                    callback: function () {
                        jQuery.post("/Archives/PraiseStatic", { "artId": praiseArtId }, function (data) {
                            if (data.Status == 1) {
                                var praisecount = parseInt(curPraise.text().match(/\d+/));
                                curPraise.text(curPraise.text().replace(praisecount, praisecount + 1));
                            } else if (data.Status == 2) {
                                alert(data.Message);
                            } else if (data.Status == 0) {
                                alert(data.Message);
                            }
                        });
                    }
                });
                niceIn(jQuery(this));
            } else if (praiseFlag == 1) {
                //2. 如果已经已赞
                jQuery("body").append("<span class='praisetip'>您已赞过~</span>");
                var tipbox = jQuery(".praisetip");
                var left = jQuery(this).offset().left;
                var top = jQuery(this).offset().top + jQuery(this).height();
                tipbox.css({
                    "position": "absolute",
                    "left": left + "px",
                    "top": top + "px",
                    "z-index": 9999,
                    "font-size": "12px",
                    "line-height": "13px",
                    "color": "red"
                });
                tipbox.animate({
                    "opacity": "0"
                }, 1200, function () {
                    tipbox.remove();
                });
            }
        });
    },

    // 回到顶端
    goTop: function () {
        jQuery(window).scroll(function () {
            jQuery(this).scrollTop() > 200 ? jQuery(".rollbar").css({
                bottom: "20px"
            }) : jQuery(".rollbar").css({
                bottom: "-50px"
            });
        });

        jQuery(".rollbar").click(function () {
            return jQuery("body,html").animate({
                scrollTop: 0
            }, 500), !1
        });
    },

    // 菜单下拉
    dropDown: function () {
        var dropDownLi = jQuery('.nav.navbar-nav li');

        dropDownLi.mouseover(function () {
            jQuery(this).addClass('open');
        }).mouseout(function () {
            jQuery(this).removeClass('open');
        });
    },

    // 小工具显示/隐藏
    panelToggle: function () {
        var toggleBtn = jQuery('.panel-toggle');
        toggleBtn.data('toggle', true);

        toggleBtn.click(function () {
            var btn = jQuery(this);
            if (btn.data('toggle')) {
                btn.removeClass('glyphicon glyphicon-upload').addClass('glyphicon glyphicon-download');
                btn.parents('div.panel').addClass('toggled');
                btn.data('toggle', false);
            } else {
                btn.removeClass('glyphicon glyphicon-download').addClass('glyphicon glyphicon-upload');
                btn.parents('div.panel').removeClass('toggled');
                btn.data('toggle', true);
            }
        });
    },

    // 小工具 删除
    panelClose: function () {
        var closeBtn = jQuery('.panel-remove');
        closeBtn.click(function () {
            var btn = jQuery(this);
            btn.parents('.panel').toggle(300);
        });
    },

    // 评论输入验证
    commentValidate: function () {
        //这个是jquery.validate.js对象级插件自己的方法,需要引入这个js
        //给$('#').validate()扩展方法传参：rules，message
        jQuery('#commentform').validate({
            onkeyup: false, //鼠标动作时不验证
            rules: {
                VName: {
                    required: true,
                    maxlength: 15
                },
                VEmail: {
                    required: true,
                    email: true,
                    maxlength: 30
                },
                CmtText: {
                    required: true,
                    maxlength: 500
                },
                LMessage: {
                    required: true,
                    maxlength: 200
                }
            },
            messages: {
                VName: {
                    required: "请输入昵称",
                    maxlength: "昵称不用太长"
                },
                VEmail: {
                    required: "邮箱不能为空！",
                    email: "邮箱格式有误",
                    maxlength: "邮箱有这么长?"
                },
                CmtText: {
                    required: "什么都没写?",
                    maxlength: "请将评论限制在500个字符以内"
                },
                LMessage: {
                    required: "什么都没写?",
                    maxlength: "请将留言限制在200个字符以内"
                }
            }
        });
    },

    // ajax评论回复
    ajaxCommentsReply: function () {
        var $ = jQuery.noConflict(); //返回jQuery的另一种表示方式。如果不反回，只写一个jQuery.noConflict();则下面的$不再是jquey中的$.jQuery.noConflict();的作用：让Jquery放弃对$的所有权

        var $commentform = $('#commentform');  //取到评论框form id=commentform
        var txt1 = '<div id="loading"> <img src="/Content/images/ico_loading2.gif" /> <span>正在提交, 请稍候...</span></div>';
        var txt2 = '<div id="error">#</div>';
        var cancel_edit = '取消编辑';
        var num = 1;
        var $comments = $('#comments-title');
        var $cancel = $('#cancel-comment-reply-link');
        var cancel_text = $cancel.text();
        var $submit = $('#commentform #submit');

        $submit.attr('disabled', false);  //将评论form中的提交按钮置为可用状态
        $body = (window.opera) ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
        //声明一个数组；不加var的话是全局变量
        comm_array = [];
        comm_array.push('');

        $('#comment').after(txt1 + txt2);   //在<textarea id="comment">后面追加上"正在提交"元素;然后将其隐藏
        $('#loading').hide();
        $('#error').hide();

        //开始绑定点击提交按钮事件：
        //无论是“评论”也好，“回复”也罢，最终都要点击提交按钮，在这个动作中统一判断到底是评论还是回复
        //iquery 1.7之后不再支持live()；换为on： 在选定的元素上绑定一个或多个事件处理函数。
        //ajax无刷新提交表单,(return flase)
        $(document).on("submit", "#commentform", function () {
            $submit.attr('disabled', true).fadeTo('slow', 0.5);
            $('#loading').slideDown();  //显示出缓冲图标

            //开始发送post请求 > ajax参数：urlcontroller = "Comment", action = "PostCmt"
            $.ajax({
                url: $("#comments").attr("data-url"),
                data: $(this).serialize(),
                type: $(this).attr('method'),
                error: function (request) {
                    $('#loading').hide();
                    $("#error").slideDown().html(request.responseText);
                    setTimeout(function () {
                        $submit.attr('disabled', false).fadeTo('slow', 1);
                        $('#error').slideUp();
                    },
                    3000);
                },
                success: function (data) {
                    //如果后台标识成功,则：；否则弹出框提示，并跳转刷新本页
                    if (data.Status == "1") {

                        //成功后先隐藏缓冲图标
                        $('#loading').hide();
                        //获取评论框<textarea></textarea>里面的内容,将其加入到comm_array数组中
                        comm_array.push($('#comment').val());

                        //锁定input昵称和input邮箱
                        //$('#vname').val();
                        //$('#vemail').val();
                        //注意：当input标签被设置为disabled时，他的数据将提交不了，只是显示字面值，不能进行交互了
                        //所以在评论成功后，把vid添加到隐藏域中
                        $('#vid').val(data.Message);
                        $('#vname').attr('disabled', true);
                        $('#vemail').attr('disabled', true);


                        //清空<textarea></textarea>
                        $('textarea').each(function () {
                            this.value = ''
                        });

                        var t = addComment,
                        cancel = t.I('cancel-comment-reply-link'),
                        temp = t.I('wp-temp-form-div'),
                        respond = t.I(t.respondId);  //没看懂t.respondId，这个responseId就是当前被回复的平论/留言id.
                        post = t.I('comment_post_ID').value;  //要回复的博文的id



                        //IE浏览器中 新增评论时样式不对的原因处在这儿：IE怎么获取不到这个value？其他浏览器都能啊
                        //问题还是处在代码中，获取隐藏域的元素应该是加上:hidden.比如$("#comment_parent:hidden").val()
                        //可是还获取不到，改为parent2后拿到了
                        ////////////parent = t.I('comment_parent').value;  //如果是回复，则取到父评论的id
                        //换一种写法:
                        parent2 = $("#comment_parent:hidden").val();

                        if (parent2 == 0) {
                            if ($comments.length) {
                                n = parseInt($comments.text().match(/\d+/));  //获取"几条评论中的"数字
                                $comments.text($comments.text().replace(n, n + 1)); //然后+1

                                //同时把文章meta信息更新：评论(n+1)
                                cmtNum = $('.commentcount a');
                                var count = parseInt(cmtNum.text().match(/\d+/));
                                cmtNum.text(cmtNum.text().replace(n, n + 1));
                            }
                        }

                        new_htm = '" id="new-comm-' + num + '"></'; //num是最开始初始化为1的变量，用于记录第几条临时评论或临时回复，在成功加一条评论后，会将它++
                        //如果是评论，则创建一个新的ol；如果是回复，则创建一个<children ol>;
                        new_htm = (parent2 == '0') ? ('\n<ol class="commentlist' + new_htm + 'ol>') : ('\n<ol class="children' + new_htm + 'ol>');

                        //div-comment-是<article>元素的id;li-comment-是<li>元素的id,他俩后面的-数值是一样的
                        div_ = (document.body.innerHTML.indexOf('div-comment-') == -1) ? '' : ((document.body.innerHTML.indexOf('li-comment-') == -1) ? 'div-' : '');

                        //在评论区域前加上一个new_htm：他就是一个ol
                        $('#respond').before(new_htm);  //自己判断一下，这里很可能为空
                        //在这个新创建的ol中追加ajax放回的data，如此看来，data里面包含的是<li><article></article></li>;后台直接拼接html
                        $('#new-comm-' + num).append(data.CoreData); //里面是data。

                        //zyn.lazyload();

                        $body.animate({
                            scrollTop: $('#new-comm-' + num).offset().top - 65
                        }, 500);

                        //10秒内不能再点击提交按钮
                        countdown();

                        num++;

                        cancel.style.display = 'none';
                        cancel.onclick = null;
                        t.I('comment_parent').value = '0';

                        parent3 = t.I('comment_parent').value;

                        if (temp && respond) {
                            temp.parentNode.insertBefore(respond, temp);
                            temp.parentNode.removeChild(temp)
                        }

                    } else if (data.Status == "0") {
                        //评论失败,请刷新后重试
                        alert(data.Message);
                        window.location.href = data.GotoUrl + "/#respond";
                    } else if (data.Status == "2") {
                        // 重名
                        alert(data.Message);
                        //$('#vname').val('');  //清空昵称填写框
                        $('#vname').focus();
                        $('#comment').val(data.CoreData);
                        $submit.attr('disabled', false).fadeTo('slow', 0.5);
                        $('#loading').hide();
                    } else if (data.Status == "3") {
                        //Message = "该文章已被删除，暂时不能进行评论、点赞等操作",
                        window.location.href = data.GotoUrl;
                    }
                }
            });
            //return false 类似于a链接的return false;防止表单真的提交（此处是ajax无刷新提交表单，不会跳到action=""）
            return false;
        }
        );

        //<div class="reply">   你想点我这个回复按钮 就得用我的这些数据；应该是在点击回复的时候，将隐藏域中的$('#comment_parent')的value从0改为被回复评论的id
        //	<a onclick="return addComment.moveForm("div-comment-2356", "2356", "respond", "763")"   //article元素的id，即父评论的id，763是文章id
        //     href="/zynblog/archives/763.html?replytocom=2356#respond" 
        //     class="comment-reply-link">
        //     回复
        //  </a>				
        //</div>
        addComment = {
            //移动评论框区域moveForm  （应用于回复按钮）
            moveForm: function (commId, parentId, respondId, postId, num) {
                var t = this,
                div,
                comm = t.I(commId), //获取那条被回复的<article id="div-comment-2356"></article>评论元素
                respond = t.I(respondId),  //respondId是整个评论框大区域的<div id="respond">
                cancel = t.I('cancel-comment-reply-link'), //获取"取消回复"Html标签对象
                parent = t.I('comment_parent'),  //获取 "向哪个父评论添加评论" 隐藏域Html标签对象
                post = t.I('comment_post_ID');  //获取 "向哪篇文章添加评论" 隐藏域Html标签对象

                num ? (
                    t.I('comment').value = comm_array[num],
                    $new_sucs = $('#success_' + num),
                    $new_sucs.hide(), $new_comm = $('#new-comm-' + num),
                    $cancel.text(cancel_edit)
                ) : $cancel.text(cancel_text);

                t.respondId = respondId;
                postId = postId || false;  //文章id

                //zyn.lazyload();

                if (!t.I('wp-temp-form-div')) {
                    div = document.createElement('div');
                    div.id = 'wp-temp-form-div';
                    div.style.display = 'none';
                    respond.parentNode.insertBefore(div, respond)
                }

                //如果能获取到那个被回复的<article>则，将respond区域整体插入到 被回复的<article>的下一个节点的前面
                //nextSibling 属性返回指定节点之后紧跟的节点，在相同的树层级中
                //否则 即comm为空时，代表是评论的意思吗？是这样判断的吗
                !comm ? (
                    temp = t.I('wp-temp-form-div'),
                    t.I('comment_parent').value = '0',
                    temp.parentNode.insertBefore(respond, temp),
                    temp.parentNode.removeChild(temp)
                ) : comm.parentNode.insertBefore(respond, comm.nextSibling); //parentNode是父节点，respond是待插入节点，m是父节点中已经存在的节点

                $body.animate({
                    scrollTop: $('#respond').offset().top - 200
                }, 400);

                if (post && postId)  //如果有"向哪篇文章添加评论" 隐藏域Html标签对象 和 postId值，则将隐藏域设为此值
                    post.value = postId;

                parent.value = parentId;  //将"向哪个父评论添加评论" 隐藏域Html标签对象 的 value值 设为 MoveForm函数形参 parentId的值
                cancel.style.display = '';

                cancel.onclick = function () {
                    $('#comment').val('');
                    var t = addComment,
                    temp = t.I('wp-temp-form-div'),
                    respond = t.I(t.respondId);
                    t.I('comment_parent').value = '0';

                    if (temp && respond) {
                        temp.parentNode.insertBefore(respond, temp);
                        temp.parentNode.removeChild(temp);
                    }
                    this.style.display = 'none';
                    this.onclick = null;
                    return false;
                };

                try {
                    t.I('comment').focus();
                }
                catch (e) { }
                return false;
            },

            I: function (e) {
                return document.getElementById(e);
            }
        };

        var wait = 10,
        submit_val = $submit.val();

        function countdown() {
            if (wait > 0) {
                $submit.val(wait);
                wait--;
                setTimeout(countdown, 1000);
            } else {
                //回复submit按钮的"发表评论"字样，并将按钮回复为可用状态
                $submit.val(submit_val).attr('disabled', false).fadeTo('slow', 1);
                wait = 10;
            }
        };
    }
};
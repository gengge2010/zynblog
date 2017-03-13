
$(document).ready(function () {
    var login = $('#loginform');
    var speed = 400;

    if ($.browser.msie == true && $.browser.version.slice(0, 3) < 10) {
        $('input[placeholder]').each(function () {

            var input = $(this);

            $(input).val(input.attr('placeholder'));

            $(input).focus(function () {
                if (input.val() == input.attr('placeholder')) {
                    input.val('');
                }
            });

            $(input).blur(function () {
                if (input.val() == '' || input.val() == input.attr('placeholder')) {
                    input.val(input.attr('placeholder'));
                }
            });
        });
    };

    $(document).on('submit', '#loginform', function () {
        $.post("/Admin/Account/Login", $(this).serialize(), function (data) {
            if (data.Status == "1") {
                window.location.href = data.CoreData;
            } else {
                layer.msg(data.Message, {
                    closeBtn: 1,
                    skin: 'layui-layer-molv',
                    shift: 4,
                    time: 2000
                }, function () {
                    $('#UserName').val('');
                    $('#PassWord').val('');
                    $('#UserName').focus();
                });
            }
        });

        return false;
    });
});
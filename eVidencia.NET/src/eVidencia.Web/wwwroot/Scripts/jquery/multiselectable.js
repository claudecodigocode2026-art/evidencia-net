/*globals $*/
/*
* Multiselectable jQuery plugin
* A Progressive Enhancement to <select multiple>
* Copyright Aki Björklund, 2009–2011
* http://akibjorklund.com/code/multiselectable/
* Version 1.0.1
* Released under the MIT license.
*/
(function ($) {
    $.fn.multiselectable = function (options) {
        var that = $(this);
        options = $.extend({
            template: '<div class="multiselectable">' +
				'<div class="m-selectable-from"><label for="m-selectable"></label>' +
				'<select multiple="multiple" id="m-selectable"></select>' +
				'</div>' +
				'<div class="m-selectable-controls">' +
                    '<button class="multis-all-right"></button>' +
					'<button class="multis-right"></button>' +
					'<button class="multis-left"></button>' +
                    '<button class="multis-all-left"></button>' +
				'</div>' +
				'<div class="m-selectable-to"><label for="m-selected"></label>' +
				'<select multiple="multiple" id="m-selected"></select>' +
				'</div>' +
			'</div>',
            selectableLabel: 'Selectable:',
            selectedLabel: 'Selected:',
            moveRightText: '>',
            moveLeftText: '<',
            moveAllRightText: '>>',
            moveAllLeftText: '<<',
            sort: true,
            sortAscending: true,
            buscador: false   //parámetro creado por necesidad del buscador.
        }, options || {});

        //disable sorting if sortOptions plugin is not found
        if (!$.fn.sortOptions) {
            options.sort = false;
        }

        return that.each(function () {
            var master = $(this);

            //make sure there are no duplicate id:s inserted into the document
            var template = options.template;
            var num = 1;
            if ($('#m-selectable').length > 0) {
                while ($('#m-selectable_' + num).length > 0) {
                    num++;
                }
                template = template.replace(/\"m-selectable\"/g, 'm-selectable_' + num);
                template = template.replace(/\"m-selected\"/g, 'm-selected_' + num);
            }

            //set the template
            master.hide().after(template);

            var size = master.attr('size');
            var m = master.next();
            var m1 = m.find('.m-selectable-from select');
            var m2 = m.find('.m-selectable-to select');

            //match the size of the reference element
            m1.attr('size', size);
            m2.attr('size', size);

            //set texts according to options 
            m.find('.m-selectable-from label').text(options.selectableLabel);
            m.find('.m-selectable-to label').text(options.selectedLabel);
            m.find('.multis-right').text(options.moveRightText);
            m.find('.multis-left').text(options.moveLeftText);
            m.find('.multis-all-right').text(options.moveAllRightText);
            m.find('.multis-all-left').text(options.moveAllLeftText);

            //move selected options to m2, unselected to m1
            $(this).find('option:selected').clone().removeAttr('selected').appendTo(m2);
            $(this).find('option:not(:selected)').clone().appendTo(m1);

            //do an initial sort to both selects
            if (options.sort) {
                m1.sortOptions(options.sortAscending);
                m2.sortOptions(options.sortAscending);
            }

            function move(from, to, master, select) {
                from.find('option:selected').removeAttr('selected').remove().appendTo(to).each(function () {
                    var matchedElem = master.find('option[value="' + $(this).val() + '"]');
                    if (select) {
                        matchedElem.attr('selected', 'selected');
                    } else {
                        matchedElem.removeAttr('selected');
                    }
                });
                if (options.sort) {
                    to.sortOptions(options.sortAscending);
                }
                return false;
            }

            function moveAll(from, to, master, select) {
                from.find('option').remove();
                to.find('option').remove();
                if (select) {
                    master.find('option').attr('selected', 'selected').clone().removeAttr('selected').appendTo(to);

                } else {
                    master.find('option').removeAttr('selected').clone().removeAttr('selected').appendTo(to);

                }
                if (options.sort) {
                    to.sortOptions(options.sortAscending);
                }
                return false;
            }

            function moveRight() {
                return move(m1, m2, master, true);
            }
            function moveLeft() {
                return move(m2, m1, master, false);
            }
            function moveAllRight() {
                if (options.buscador == true) {
                    moveAllRightBuscador(m1, m2, master);
                }
                else {
                    return moveAll(m1, m2, master, true);
                }
                return false;
            }

            /*Añadimos este método al pluggin por necesidades del buscador.
            El buscador convierte los options en spans en los casos de options que no se deben mostrar porque no son 
            resultados que cumplan las condiciones del filtro. Por lo tanto hemos creado este método para que en el caso
            en que la lista viene con spans no los modifique a options y tenga el comportamiento que deseamos para el 
            buscador. 
            Este método será usado únicamente si el parámetro "buscador" se lo enviamos a true. Por defecto usará el método
            que usaba
            */
            function moveAllRightBuscador(m1, m2, master) {

                var optionsM1 = m1.find('option');

                optionsM1.each(function () {
                    master.find('option[value=' + $(this).val() + ']').attr('selected', 'selected').clone().removeAttr('selected').appendTo(m2);
                    m1.find('option[value=' + $(this).val() + ']').remove();
                });

                if (options.sort) {
                    m2.sortOptions(options.sortAscending);
                }
                return false;
            }

            function moveAllLeft() {
                return moveAll(m2, m1, master, false);
            }

            //set all the events that trigger a move
            m.find('.multis-right').click(moveRight);
            m.find('.multis-left').click(moveLeft);
            m.find('.multis-all-right').click(moveAllRight);
            m.find('.multis-all-left').click(moveAllLeft);
            m1.dblclick(moveRight);
            m2.dblclick(moveLeft);
            m1.keydown(function (event) {
                if (event.keyCode === 13) {
                    moveRight();
                }
            });
            m2.keydown(function (event) {
                if (event.keyCode === 13) {
                    moveLeft();
                }
            });
        });
    };
})(jQuery);

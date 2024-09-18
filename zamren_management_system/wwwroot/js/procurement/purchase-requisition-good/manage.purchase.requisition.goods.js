$(document).ready(function () {
    function renumberItems() {
        $('#goodsContainer .good-item').each(function (index) {
            $(this).find('.item-number').text((index + 1) + `.`);
        });
    }

    $('#addGood').click(function () {
        let goodItem = `
            <tr class="good-item">
                <td class="item-number"></td>
                <td>
                    <textarea class="form-control large-textarea" maxlength="255" name="itemDescription[]" rows="3" placeholder="Describe Item" required></textarea>
                </td>
                <td>
                    <input type="number" class="form-control" name="quantity[]" min="1" placeholder="0" required>
                </td>
                <td>
                    <input type="number" class="form-control" name="unitPrice[]" min="1" placeholder="0.00" required>
                </td>
                <td>
                    <input type="file" class="form-control" name="goodFile[]" accept=".png,.jpg,.jpeg,.pdf">
                </td>
                <td>
                    <button type="button" class="btn btn-sm btn-danger removeGood">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
        $('#goodsContainer').append(goodItem);
        renumberItems();
    });

    $(document).on('click', '.removeGood', function () {
        $(this).closest('.good-item').remove();
        renumberItems();
    });

    renumberItems();
});
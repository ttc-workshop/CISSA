
cissa = {
    homeUrl: "/"
};

function updateMainMenuState(e) {
    var treeView = $(this).data("tTreeView");

    var expandedNodes = $.cookie("ExpandedNodes");
    expandedNodes = expandedNodes ? expandedNodes.split(";") : [];

    var treeNode = e.item;
    var nodeValue = treeView.getItemValue(treeNode);
    var itemIndex = $.inArray(nodeValue, expandedNodes);

    if (e.type === "expand" && itemIndex === -1)
        expandedNodes.push(nodeValue);
    else if (e.type === "collapse" && itemIndex >= 0)
        expandedNodes.splice(itemIndex, 1);

    $.cookie("ExpandedNodes", expandedNodes.join(";"));
}

function onChangeMasterComboBox() {
    var comboBox = $(this).data("tComboBox");
    var value = comboBox.value();
    $("#overlay").show();
    $.ajax({
        url: cissa.homeUrl + "Form/UpdateMasterControl/" + comboBox.element.id,
        dataType: "json",
        data: { value: value },
        success: updateComboBoxDependentsCallback
    });
}
function onChangeMasterDropDownList() {
    var comboBox = $(this).data("tDropDownList");
    var value = comboBox.value();
    $("#overlay").show();
    $.ajax({
        url: cissa.homeUrl + "Form/UpdateMasterControl/" + comboBox.element.id,
        dataType: "json",
        data: { value: value },
        success: updateComboBoxDependentsCallback
    });
}
function updateComboBoxDependentsCallback(json) {
    if (json /*&& json.Data*/)
        for (var element in json/*.Data*/) {
            if (json/*.Data*/.hasOwnProperty(element)) {
                var updateData = json[element];
                if (updateData && updateData.id) {
                    updateDependentComboBox(updateData.id, updateData.value, updateData.items);
                }
            }
        }
    $("#overlay").hide();
}

function updateDependentComboBox(id, value, items) {
    var comboBox = $("#" + id).data("tComboBox");
    if (!comboBox)
        comboBox = $("#" + id).data("tDropDownList");

    if (comboBox) {
        if (value)
            comboBox.value(value);
        if (items)
            comboBox.dataBind(items);
        //comboBox.reload();
    }
}
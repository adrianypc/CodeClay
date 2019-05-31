function GotoUrl(navigateUrl) {
    if (navigateUrl) {
        var tokens = navigateUrl.split(LIST_SEPARATOR);
        var url = tokens[0];
        var position = "";
        if (tokens.length > 1) {
            position = tokens[1];
        }

        switch (position) {
            case "Popup":
                var width;
                var height;

                if (tokens.length > 3) {
                    width = tokens[2];
                    height = tokens[3];
                }

                window.ShowPopup(url, width, height);
                break;

            case "Parent":
                window.parent.HidePopup();
                break;

            case "NewTab":
                window.open(url, "_blank");
                break;

            default:
                window.open(url, "_self");
                break;
        }
    }
}
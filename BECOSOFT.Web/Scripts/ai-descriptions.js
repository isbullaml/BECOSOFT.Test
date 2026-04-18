$(document).ready(function () {
    var generatedPendingApprovalExist = $('[data-id=ai-pending-approval-exist]').val();
    if (generatedPendingApprovalExist === "True") {
        checkIfGeneratedPendingRequestExist();
        setInterval(checkIfGeneratedPendingRequestExist, 1000);
    }
});

function checkIfGeneratedPendingRequestExist() {
    $("[data-id=ai-helper]").fadeOut(100).fadeIn(100).fadeOut(100).fadeIn(100);
}

$("[data-id=ai-helper]").on("click", function (e) {    
    $('#becosoftAIHelperModal').modal('show');

    var pendingApprovalExist = $('[data-id=ai-pending-approval-exist]').val();
    if (pendingApprovalExist === "True") {
        var metaTitle = $('#MetaTitleGeneratedArticlePendingRequest').val();
        var longDescription = $('#LongDescriptionGeneratedArticlePendingRequest').val();
        var metaDescription = $('#MetaDescriptionGeneratedArticlePendingRequest').val();
        var metaKeywords = $('#MetaKeywordsGeneratedArticlePendingRequest').val();

        $('#ai-helper-meta-title-result').text(metaTitle);
        $('#ai-helper-meta-description-result').text(metaDescription);
        $('#ai-helper-long-description-result').text(longDescription);
        $('#ai-helper-meta-keywords-result').text(metaKeywords);
        $('[data-id=ai-helper-long-description-field] .fr-element.fr-view').text(longDescription);
        $('[data-id=ai-helper-long-description-field] .fr-placeholder').text("");

        $('#select-descriptions-to-generate').removeClass('show');
        $('#ai-helper-descriptions-result').addClass('show');
    }
});

$("[data-id=generate-descriptions-btn]").on("click", function (e) {
    $('[data-id=generate-descriptions-btn] i.spinning').removeClass('d-none');

    var url = $(this).data('url');

    var languageID = $('#main-content #LanguageID').find(":selected").val();
    $('[data-id=current-language-ai-helper-id]').val(languageID);

    $.ajax({
        type: "POST",
        url: url,
        data: $('[data-id=generate-descriptions-form]').serialize(),
        success: function (resp) {
            $('[data-id=generate-descriptions-btn] i.spinning').addClass('d-none');
            $('#select-descriptions-to-generate').removeClass('show');
            $('#ai-helper-meta-title-result').val(resp.MetaTitle);
            $('#ai-helper-meta-description-result').val(resp.MetaDescription);
            $('#ai-helper-long-description-result').val(resp.Description);
            $('[data-id=ai-helper-long-description-field] .fr-element.fr-view').text(resp.Description);
            $('[data-id=ai-helper-long-description-field] .fr-placeholder').text("");

            if (resp.MetaKeywords !== null) {
                $('#ai-helper-meta-keywords-result').val(resp.MetaKeywords.join(', '));
            }

            $('#ai-helper-descriptions-result').addClass('show');
        },
        error: function () {
            $('[data-id=generate-descriptions-btn] i.spinning').addClass('d-none');
        }
    });
});

$(document).on("click", "[data-id=confirm-overwrite-button]", function (e) {
    var metaTitle = $('#ai-helper-meta-title-result').val();
    var longDescription = $('#ai-helper-long-description-result').val();
    var metaDescription = $('#ai-helper-meta-description-result').val();
    var metaKeywords = $('#ai-helper-meta-keywords-result').val();

    if (metaTitle !== "") {
        $('#main-content #MetaTitle').text(metaTitle);
    }
    if (longDescription !== "") {
        $('#main-content #LongDescription').val(longDescription);
        $('[data-id=article-information-long-description]').parent().find(".fr-element.fr-view").text(longDescription);
        $('[data-id=article-information-long-description]').parent().find(".fr-placeholder").text("");
    }
    if (metaDescription !== "") {
        $('#main-content #MetaDescription').text(metaDescription);
    }
    if (metaKeywords !== "") {
        $('#main-content #MetaKeywords').text(metaKeywords);
    }

    $('#becosoftAIHelperModal').modal('hide');
});

$("[data-id=translate-ai]").on("click", function (e) {
    var selectedLanguageID = $('#main-content #LanguageID').find(":selected").val();
    $('[data-id=translate-languages-dropdown] option[value="' + selectedLanguageID + '"]').remove();
    $('#becosoftAITranslationModal').modal('show');
});

$(document).on("click", "[data-id=translate-confirm-overwrite-button]", function (e) {
    var metaTitle = $('#ai-translate-meta-title-target').val();
    var longDescription = $('#ai-translate-long-description-target').text();
    var metaDescription = $('#ai-translate-meta-description-target').val();
    var metaKeywords = $('#ai-translate-meta-keywords-target').val();

    if (metaTitle !== "") {
        $('#main-content #MetaTitle').text(metaTitle);
    }
    console.log(longDescription);
    if (longDescription !== "") {
        $('#main-content #LongDescription').val(longDescription);
        $('[data-id=article-information-long-description]').parent().find(".fr-element.fr-view").text(longDescription);
        $('[data-id=article-information-long-description]').parent().find(".fr-placeholder").text("");
    }
    if (metaDescription !== "") {
        $('#main-content #MetaDescription').text(metaDescription);
    }
    if (metaKeywords !== "") {
        $('#main-content #MetaKeywords').text(metaKeywords);
    }

    $('#becosoftAITranslationModal').modal('hide');
});

$(document).on('change', '[data-id=translate-languages-dropdown]', function () {
    var languageID = $('[data-id=translate-languages-dropdown] option:selected').val();
    var url = $(location).attr("href").slice(0, -1) + languageID;
    location.href = url;
});
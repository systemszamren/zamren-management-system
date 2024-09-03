$(document).ready(function () {

    //get encrypted reference from url if url contains ?reference=xxx parameter
    let url = new URL(window.location.href);
    let reference = url.searchParams.get("reference");
    let data = {reference: reference ? reference : null};

    let commentsContainer = $('.chat-box');

    ajaxDataRequest('/api/workflow/task/comment/get-by-reference', 'POST', data, null, function (err, data) {
        if (err) {
            // Handle error
            // console.error('An error occurred:', err);
            commentsContainer.append('<p>No comments at the moment</p>');
        } else {
            // Handle success
            let comments = data.comments;
            commentsContainer.empty(); // Clear existing comments

            if (comments && comments.length > 0) {
                //check if is first comment
                comments.forEach((comment, index) => {
                    let commentClass = index === 0 ? 'chat-message p-3 rounded chat-most-recent' : 'chat-message p-3 rounded';
                    let commentHtml = `
                        <div class="${commentClass}">
                            <a href="#" target="_blank" class="fw-bold">${comment.commentedBy.fullName}</a>
                            <small class="text-muted ms-2">${comment.timeAgo}</small>
                            <p class="mb-0">${comment.comment}</p>
                            <span class="badge bg-light chat-step-name">${comment.step.name}</span>
                            <span class="badge bg-success text-white chat-step-action">${comment.step.action}</span>
                    `;

                    if (comment.wkfAttachments && comment.wkfAttachments.length > 0) {
                        comment.wkfAttachments.forEach(attachment => {
                            commentHtml += `
                                <a href="#" class="badge text-primary bg-light chat-step-attachment" title="${attachment.systemAttachment.originalFileName}" data-bs-toggle="modal" data-bs-target="#fileModal" data-file="${attachment.systemAttachment.systemFileName}">
                                    <i class="fa fa-paperclip text-primary"></i> ${attachment.systemAttachment.originalFileName}
                                </a>
                            `;
                        });
                    }

                    commentHtml += `</div>`;
                    commentsContainer.append(commentHtml);
                });
            } else {
                commentsContainer.append('<p>No comments at the moment</p>');
            }
        }
    });
    
});
import {inject} from 'aurelia-framework';
import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import '/styles/custom.css!';
import {Router} from 'aurelia-router';


export class Feeds {
  hello = 'Welcome to Aurelia!';

  static inject() {
    return [Router];
  }

  addFeedGroup(e){
    this.router.navigate("feedgroup/create");
  }

  constructor(router) {
    this.router = router;
  }

  feedClick(e) {
    this.router.navigate("feeds/view/1");
  }

  activate() {
    $.ajax({
      url: "/api/feeds",
      cache: false,
      headers: new auth().getAuthHttpHeader(),
      dataType: 'json'
    }).then(function(response) {
      self.feedsLoaded(true);
      self.pageCount(response.TotalPages);
      self.currentPage(pageNumber);

      var mapping = {
        create: function(options) {
          return databindingFeed(options.data);
        }
      };

      ko.mapping.fromJS(response.Results, mapping, self.feeds);


    }).fail(function (xmlHttpRequest, textStatus, errorThrown) {
      self.feedsLoaded(true);

      if (xmlHttpRequest.status === 401) {
        router.navigate("#signin");
      }
    });
  }

  attached() {
    // called when View is attached, you are safe to do DOM operations here
  }
}

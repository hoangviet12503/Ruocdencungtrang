using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Data;

namespace MidAutumn.UI
{
    /// <summary>Visual for one friend marker on the map path.</summary>
    public class FriendAvatarView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private Text nameLabel;

        public void Bind(FriendProgress friend)
        {
            nameLabel.text = friend.displayName;
            if (friend.avatar != null)
                avatarImage.sprite = friend.avatar;
            // else: caller is expected to async-load friend.avatarUrl into avatarImage.sprite.
        }
    }
}

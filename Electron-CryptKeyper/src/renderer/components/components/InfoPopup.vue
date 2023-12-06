<template>
<div class="info-screen" v-if="isOpen">
    <div class="info-content" :class="{'open': isOpenDelayed}">
        <h2>{{ type[0].toUpperCase() + type.slice(1) }}</h2>
        <section style="padding: 20px;">
          <!-- MAKE THESE COPY WHEN YOU CLICK EITHER SIDE -->
          <!-- ALSO MAKE THEM BIGGER?? ARE THEY MAYBE TOO BIG NOW??? -->
            <div  @click="copyValue(index)" class="fullField" v-for="(field, index) in fieldTitles" :key="index">
                <div style="font-weight: bolder;">{{ field }}</div>
                <div>{{ fieldValues[index] }}</div>
            </div>
        </section>  
    </div>
    <div class="info-clickbox" @click="closeInfoPopup"></div> 
  </div>
</template>

<script>
export default {
  data() {
    return {
        isOpenDelayed: false,
        type: '',
        fieldTitles: [],
        fieldValues: [],
    };
  },
  methods: {
    closeInfoPopup() {
      this.$emit('close-info-popup');
    },
    copyValue(index) {
        let text = this.fieldValues[index];

        try {
            if(!navigator.clipboard) throw('Unsupported browser');
            navigator.clipboard.writeText(text);
        } catch(error) {
            // Fallback for insecure browsers / Localhost testing
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
        }

        this.$emit('copied')
    },
  },
    props: [ 'isOpen', 'chosenItem'],
    watch: {
        async isOpen() {
            // Wait .1 seconds for isOpen to be set to true before setting isOpenClass to true
            await new Promise((resolve) => setTimeout(resolve, 100));
            this.isOpenDelayed = this.isOpen;
        },
        chosenItem() {
            // Change how data is displayed here

            switch (this.chosenItem.itemType) {
                case 'password':
                    this.type = 'password';
                    this.fieldTitles = ['URL/Location', 'Email', 'Username', 'Password'];
                    this.fieldValues = [this.chosenItem.item.plaintextLocation, this.chosenItem.item.plaintextEmail, this.chosenItem.item.plaintextUsername, this.chosenItem.item.plaintextIVPass];
                    break;
                case 'payment':
                    this.type = 'payment';
                    this.fieldTitles = ['Card Name', 'Card Number', 'CVV', 'Expiration Date'];
                    this.fieldValues = [this.chosenItem.item.plaintextCardName, this.chosenItem.item.plaintextCardNumber, this.chosenItem.item.plaintextCardCVV, this.chosenItem.item.plaintextCardExpMonth + '/' + this.chosenItem.item.plaintextCardExpYear];
                    break;
                case 'note':
                    this.type = 'note';
                    this.fieldTitles = ['Title', 'Note'];
                    this.fieldValues = [this.chosenItem.item.plaintextTitle, this.chosenItem.item.plaintextNote];
                    break;
                default:
                    break;
            }
        },
    },
};
</script>

<style scoped>
.info-screen {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: right;
  align-items: right;
  z-index: 20;
}

.info-clickbox {
  width: 60%;
  height: 100%;
  position: fixed;
  left: 0;
  margin-left: auto;
}

.info-content {
  width: 0%;
  height: 100%;
  position: fixed;
  right: 0;
  margin-left: auto;
  background-color: aliceblue;
  transition: width 0.3s;
}

@media only screen and (max-width: 767px) {
  .info-content {
    width: 100%; /* Set the width to 100% for mobile devices */
    transition: height 0.3s;
    height: 0%;
    bottom: 0;
    text-align: center;
  }
}

.info-content.open {
  width: 40%;
  margin-top: 0px;
}

@media only screen and (max-width: 767px) {
  .info-content.open {
    width: 100%; /* Set the width to 100% for mobile devices */
    height: 100%;
  }
}

.fullField {
    display: flex;
    justify-content: space-between;
    margin: 10px;
    padding: 10px;
    border-bottom: 1px solid #c1c1c1;
}

.fullField div {
    margin: 5px;
    font-size:x-large;
}

.info-content h2 {
  margin: 0;
  padding: 15px;
  background-color: #fff;
  border-bottom: 1px solid #c1c1c1;
}
</style>

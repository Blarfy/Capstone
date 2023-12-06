<template>
    <div>
        <div class="content flex-container" :class="{ 'open': isSidebarOpen}">
            <div class="column" v-for="(field, count) in fieldNames" :key="field">
                <h2>{{ field }}</h2> 
                <section class="item-holder" v-for="(item, index) in filteredFields[count]" :key="item.id">
                    <p :key="item.id" :class="{ 'hidden-text': isFieldHidden[count][index]}" @click="copyField(filteredFields[count][index])">{{ filteredFields[count][index] }} </p>
                    <button v-if="hiddenFields[count]" class="show-button" @click="showField(count,index)">👁</button>



                </section>
            </div>

            <div class="button-column">
                <h2>Actions </h2>
                <section class="item-holder" v-for="(item, index) in filteredFields[0]" :key="item.id">
                    <button class="share-button" @click="chooseShare(index)">📤</button>
                    <button class="delete-button" @click="chooseDelete(index)">🗑️</button>
                </section>
            </div>
        </div>
        <ConfirmDelete v-if="confirmingDelete" @confirm-delete="deleteItem" @cancel-delete="toggleDelete"></ConfirmDelete>
        <DynamicForm v-if="confirmingShare" :title="'Share To...'" :count="1" :labels="['Username']" :required-fields="[true]" @form-submitted="shareItem" @close-btn="toggleShare"></DynamicForm>
    </div>

</template>

<script>
import ConfirmDelete from './ConfirmDelete.vue';
import DynamicForm from './DynamicForm.vue';

    export default {
    name: 'DynamicItemDisplay',
    props: ['isSidebarOpen', 'fieldNames', 'filteredFields', 'hiddenFields'],
    data() {
        return {
            isFieldHidden: [], // Array of arrays of booleans indicating whether or not a field is hidden
            confirmingDelete: false,
            confirmingShare: false,
            selected: -1,
        };
    },
    computed: {},
    created() {
        this.setupItemFields();
    },
    watch: {
        filteredFields() {
            this.setupItemFields();
        }
    },
    methods: {
        showField(count, index) {
            this.$set(this.isFieldHidden[count], index, !this.isFieldHidden[count][index]);
        },

        setupItemFields() {
            let tempHidden = [];
            for (let i = 0; i < this.fieldNames.length; i++) {
                if (this.hiddenFields[i]) {
                    let temp = [];
                    for (let j = 0; j < this.filteredFields[i].length; j++) {
                        temp.push(true);
                    }
                    tempHidden.push(temp);
                }
                else {
                    tempHidden.push(Array(this.filteredFields[i].length).fill(false));
                }
            }
            this.$set(this, 'isFieldHidden', tempHidden);
        },

        copyField(text) {
            try {
                if (!navigator.clipboard)
                    throw ('Unsupported browser');
                navigator.clipboard.writeText(text);
            }
            catch (error) {
                // Fallback for insecure browsers / Localhost testing
                const textArea = document.createElement('textarea');
                textArea.value = text;
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();
                document.execCommand('copy');
                document.body.removeChild(textArea);
            }
            this.$emit('copied');
        },

        toggleDelete() {
            this.confirmingDelete = !this.confirmingDelete;
        },

        chooseDelete(index) {
            this.selected = index;
            this.toggleDelete();
        },

        toggleShare() {
            this.confirmingShare = !this.confirmingShare;
        },

        chooseShare(index) {
            this.selected = index;
            this.toggleShare();
        },

        deleteItem() {
            this.$emit('delete-item', this.selected);
            this.toggleDelete();
        },

        shareItem(email) {
            this.$emit('share-item', this.selected, email);
            this.toggleShare();
        }
    },
    components: { ConfirmDelete, DynamicForm }
}
</script>

<style>
.flex-container {
    display: flex;
    flex-wrap: nowrap;
    justify-content: space-around;
    align-items: stretch;
    align-content: center;
    flex: 1;
}

.column {
  margin-top: 20px;
  padding: 15px;
  width: 15%;
  border: 1px solid #ccc;
  flex: 1;
  align-items: start;
}

.button-column {
    margin-top: 20px;
    padding: 15px;
    width: 10%;
    border: 1px solid #ccc;
    flex-shrink: 6;
    display: flex;
    flex-direction: column;
}

.button-column h2 {
    font-weight: bold;
    font-size: 28px;
    border-bottom: 2px solid darkgray;
    text-align: center;
}

.column h2 {
  font-weight: bold;
  font-size: 28px;
  border-bottom: 2px solid darkgray;
  text-align: center;
}

.column p {
  width: 98%;
  white-space: nowrap;
  margin: 10px;
  border-radius: 8px;
  font-weight: bold;
  padding: 5px;
  padding-left: 0px;
  padding-right: 0px;
  background-color: #c1c1c1;
  text-align: center;
  font-size: large;
  overflow:hidden;
}

.delete-button {
    border: 2px solid lightgrey;
    background-color: darkslategrey;
    position: relative;
    color: white;
    padding: 8px;
    text-align: center;
    font-size: large;
}

.delete-button:hover {
    background-color: crimson;
    color: black;
}

.share-button {
    border: 2px solid lightgrey;
    background-color: darkslategrey;
    position: relative;
    color: white;
    padding: 8px;
    text-align: center;
    font-size: large;
}

.share-button:hover {
    background-color: aquamarine;
    color: black;
}

.decrypted-item {
    display: flex;
    flex-wrap: nowrap;
    flex-direction: row;
    justify-content: space-around;
    align-items: center;
    align-content: center;
    border: 1px solid black;
    width: 100%;
    border-radius: 5px;
    padding: 5px;
    margin: 8px;
}

.item-holder {
    display: flex;
    flex-wrap: nowrap;
    flex-direction: row;
    justify-content: space-around;
    align-items: center;
    align-content: center;
    height: 60px;
    width: 100%;
}

.item-field {
    width: 100%;
    white-space: nowrap;
    border-radius: 8px;
    font-weight: bold;
    font-size: large;
    padding: 5px;
    background-color: #c1c1c1;
    text-align: center;
}

.show-button {
    margin-left: auto;
    margin-right: 0px;
    border: 2px solid transparent;
    background-color: transparent;
    position: relative;
    font-size: xx-large;
    color: grey;
}

.hidden-text {
    -webkit-text-security: disc;
}
</style>